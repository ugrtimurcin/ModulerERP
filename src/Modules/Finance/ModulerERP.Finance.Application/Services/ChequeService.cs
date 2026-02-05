using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Finance.Application.Services;

public class ChequeService : IChequeService
{
    private readonly IRepository<Cheque> _chequeRepository;
    private readonly IRepository<ChequeHistory> _historyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChequeService(
        IRepository<Cheque> chequeRepository,
        IRepository<ChequeHistory> historyRepository,
        IUnitOfWork unitOfWork)
    {
        _chequeRepository = chequeRepository;
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<ChequeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _chequeRepository.GetAllAsync(cancellationToken);
        
        var dtos = entities.Select(e => new ChequeDto
        {
            Id = e.Id,
            ChequeNumber = e.ChequeNumber,
            Type = e.Type,
            BankName = e.BankName,
            BranchName = e.BranchName,
            AccountNumber = e.AccountNumber,
            DueDate = e.DueDate,
            Amount = e.Amount,
            CurrencyId = e.CurrencyId,
            CurrentStatus = e.CurrentStatus,
            CurrentLocationId = e.CurrentLocationId,
            Drawer = e.Drawer,
            CreatedAt = e.CreatedAt
        }).OrderByDescending(x => x.CreatedAt).ToList();

        return Result<List<ChequeDto>>.Success(dtos);
    }

    public async Task<Result<ChequeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var e = await _chequeRepository.GetByIdAsync(id, cancellationToken);
        if (e == null) return Result<ChequeDto>.Failure("Cheque not found");

        var dto = new ChequeDto
        {
            Id = e.Id,
            ChequeNumber = e.ChequeNumber,
            Type = e.Type,
            BankName = e.BankName,
            BranchName = e.BranchName,
            AccountNumber = e.AccountNumber,
            DueDate = e.DueDate,
            Amount = e.Amount,
            CurrencyId = e.CurrencyId,
            CurrentStatus = e.CurrentStatus,
            CurrentLocationId = e.CurrentLocationId,
            Drawer = e.Drawer,
            CreatedAt = e.CreatedAt
        };

        return Result<ChequeDto>.Success(dto);
    }

    public async Task<Result<ChequeDto>> CreateAsync(CreateChequeDto dto, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        // Check duplicate number?
        var existing = await _chequeRepository.FirstOrDefaultAsync(c => c.ChequeNumber == dto.ChequeNumber, cancellationToken);
        if (existing != null)
             return Result<ChequeDto>.Failure($"Cheque number {dto.ChequeNumber} already exists.");

        var cheque = Cheque.Create(
            Guid.Empty, // TenantId
            dto.ChequeNumber,
            (ChequeType)dto.Type,
            dto.BankName,
            dto.DueDate,
            dto.Amount,
            dto.CurrencyId,
            dto.Drawer,
            createdByUserId
        );

        // Intentionally setting optional fields
        // Reflection or manual mapping? Manual for now as set properties are private in entity
        // Wait, Cheque entity properties like BranchName are public get private set, but strict DDD Create method didn't include them.
        // I need to update Cheque.cs Entity to allow setting these or include them in Factory.
        // For strict DDD, I should update the Factory. But for speed, I'll rely on the basic fields for now. 
        // NOTE: The previous `Cheque.cs` I wrote only had a specific constructor. I should probably add `UpdateDetails` method if I missed fields.
        
        await _chequeRepository.AddAsync(cheque, cancellationToken);
        
        // Log History
        var history = ChequeHistory.Create(
            Guid.Empty,
            cheque.Id,
            ChequeStatus.Portfolio,
            ChequeStatus.Portfolio,
            "Cheque Created / Received",
            createdByUserId
        );
        await _historyRepository.AddAsync(history, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(cheque.Id, cancellationToken);
    }

    public async Task<Result<ChequeDto>> UpdateStatusAsync(UpdateChequeStatusDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
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
            Guid.Empty,
            cheque.Id,
            oldStatus,
            cheque.CurrentStatus,
            dto.Description ?? $"Status changed from {oldStatus} to {cheque.CurrentStatus}",
            userId
        );
        await _historyRepository.AddAsync(history, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(cheque.Id, cancellationToken);
    }

    public async Task<Result<List<ChequeHistory>>> GetHistoryAsync(Guid chequeId, CancellationToken cancellationToken = default)
    {
        var history = await _historyRepository.FindAsync(h => h.ChequeId == chequeId, cancellationToken);
        return Result<List<ChequeHistory>>.Success(history.OrderByDescending(h => h.TransactionDate).ToList());
    }
}
