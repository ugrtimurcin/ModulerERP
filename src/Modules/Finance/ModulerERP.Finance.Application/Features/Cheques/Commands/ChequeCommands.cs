using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.Cheques.Commands;

// ── Create Cheque ──
public record CreateChequeCommand(CreateChequeDto Dto, Guid CreatedByUserId) : IRequest<Result<ChequeDto>>;

public class CreateChequeCommandHandler : IRequestHandler<CreateChequeCommand, Result<ChequeDto>>
{
    private readonly IRepository<Cheque> _chequeRepository;
    private readonly IRepository<ChequeHistory> _historyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublisher _publisher;

    public CreateChequeCommandHandler(
        IRepository<Cheque> chequeRepository,
        IRepository<ChequeHistory> historyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPublisher publisher)
    {
        _chequeRepository = chequeRepository;
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _publisher = publisher;
    }

    public async Task<Result<ChequeDto>> Handle(CreateChequeCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var createdByUserId = request.CreatedByUserId;

        var existing = await _chequeRepository.FirstOrDefaultAsync(c => c.ChequeNumber == dto.ChequeNumber, cancellationToken);
        if (existing != null)
             return Result<ChequeDto>.Failure($"Cheque number {dto.ChequeNumber} already exists.");

        var cheque = Cheque.Create(
            _currentUserService.TenantId, // TenantId
            dto.ChequeNumber,
            (ChequeType)dto.Type,
            dto.BankName,
            dto.DueDate,
            dto.Amount,
            dto.CurrencyId,
            dto.Drawer,
            createdByUserId
        );

        await _chequeRepository.AddAsync(cheque, cancellationToken);
        
        var history = ChequeHistory.Create(
            _currentUserService.TenantId,
            cheque.Id,
            ChequeStatus.Portfolio,
            ChequeStatus.Portfolio,
            "Cheque Created / Received",
            createdByUserId
        );
        await _historyRepository.AddAsync(history, cancellationToken);

        await _publisher.Publish(new ModulerERP.Finance.Domain.Events.ChequeCreatedEvent(
            cheque.TenantId,
            cheque.Id,
            cheque.Amount,
            cheque.ChequeNumber,
            cheque.Type
        ), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to dto inline
        var resultDto = new ChequeDto
        {
            Id = cheque.Id,
            ChequeNumber = cheque.ChequeNumber,
            Type = cheque.Type,
            BankName = cheque.BankName,
            BranchName = cheque.BranchName,
            AccountNumber = cheque.AccountNumber,
            DueDate = cheque.DueDate,
            Amount = cheque.Amount,
            CurrencyId = cheque.CurrencyId,
            CurrentStatus = cheque.CurrentStatus,
            CurrentLocationId = cheque.CurrentLocationId,
            Drawer = cheque.Drawer,
            CreatedAt = cheque.CreatedAt
        };

        return Result<ChequeDto>.Success(resultDto);
    }
}

// ── Update Cheque Status ──
public record UpdateChequeStatusCommand(UpdateChequeStatusDto Dto, Guid UserId) : IRequest<Result<ChequeDto>>;

public class UpdateChequeStatusCommandHandler : IRequestHandler<UpdateChequeStatusCommand, Result<ChequeDto>>
{
    private readonly IRepository<Cheque> _chequeRepository;
    private readonly IRepository<ChequeHistory> _historyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublisher _publisher;

    public UpdateChequeStatusCommandHandler(
        IRepository<Cheque> chequeRepository,
        IRepository<ChequeHistory> historyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPublisher publisher)
    {
        _chequeRepository = chequeRepository;
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _publisher = publisher;
    }

    public async Task<Result<ChequeDto>> Handle(UpdateChequeStatusCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var userId = request.UserId;

        var cheque = await _chequeRepository.GetByIdAsync(dto.ChequeId, cancellationToken);
        if (cheque == null) return Result<ChequeDto>.Failure("Cheque not found");

        var oldStatus = cheque.CurrentStatus;
        try
        {
            cheque.UpdateStatus((ChequeStatus)dto.NewStatus, dto.NewLocationId, userId);
        }
        catch (InvalidOperationException ex)
        {
            return Result<ChequeDto>.Failure(ex.Message);
        }

        _chequeRepository.Update(cheque);

        var history = ChequeHistory.Create(
            _currentUserService.TenantId,
            cheque.Id,
            oldStatus,
            cheque.CurrentStatus,
            dto.Description ?? $"Status changed from {oldStatus} to {cheque.CurrentStatus}",
            userId
        );
        await _historyRepository.AddAsync(history, cancellationToken);

        await _publisher.Publish(new ModulerERP.Finance.Domain.Events.ChequeStatusUpdatedEvent(
            cheque.TenantId,
            cheque.Id,
            oldStatus,
            cheque.CurrentStatus,
            cheque.Amount,
            cheque.ChequeNumber
        ), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var resultDto = new ChequeDto
        {
            Id = cheque.Id,
            ChequeNumber = cheque.ChequeNumber,
            Type = cheque.Type,
            BankName = cheque.BankName,
            BranchName = cheque.BranchName,
            AccountNumber = cheque.AccountNumber,
            DueDate = cheque.DueDate,
            Amount = cheque.Amount,
            CurrencyId = cheque.CurrencyId,
            CurrentStatus = cheque.CurrentStatus,
            CurrentLocationId = cheque.CurrentLocationId,
            Drawer = cheque.Drawer,
            CreatedAt = cheque.CreatedAt
        };

        return Result<ChequeDto>.Success(resultDto);
    }
}
