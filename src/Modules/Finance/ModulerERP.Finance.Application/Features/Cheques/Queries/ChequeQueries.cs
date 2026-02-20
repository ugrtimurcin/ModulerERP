using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.Cheques.Queries;

// ── Get All Cheques ──
public record GetChequesQuery : IRequest<Result<List<ChequeDto>>>;

public class GetChequesQueryHandler : IRequestHandler<GetChequesQuery, Result<List<ChequeDto>>>
{
    private readonly IRepository<Cheque> _repository;

    public GetChequesQueryHandler(IRepository<Cheque> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ChequeDto>>> Handle(GetChequesQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        
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
}

// ── Get Cheque By Id ──
public record GetChequeByIdQuery(Guid Id) : IRequest<Result<ChequeDto>>;

public class GetChequeByIdQueryHandler : IRequestHandler<GetChequeByIdQuery, Result<ChequeDto>>
{
    private readonly IRepository<Cheque> _repository;

    public GetChequeByIdQueryHandler(IRepository<Cheque> repository)
    {
        _repository = repository;
    }

    public async Task<Result<ChequeDto>> Handle(GetChequeByIdQuery request, CancellationToken cancellationToken)
    {
        var e = await _repository.GetByIdAsync(request.Id, cancellationToken);
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
}

// ── Get Cheque History ──
public record GetChequeHistoryQuery(Guid ChequeId) : IRequest<Result<List<ChequeHistory>>>;

public class GetChequeHistoryQueryHandler : IRequestHandler<GetChequeHistoryQuery, Result<List<ChequeHistory>>>
{
    private readonly IRepository<ChequeHistory> _historyRepository;

    public GetChequeHistoryQueryHandler(IRepository<ChequeHistory> historyRepository)
    {
        _historyRepository = historyRepository;
    }

    public async Task<Result<List<ChequeHistory>>> Handle(GetChequeHistoryQuery request, CancellationToken cancellationToken)
    {
        var history = await _historyRepository.FindAsync(h => h.ChequeId == request.ChequeId, cancellationToken);
        return Result<List<ChequeHistory>>.Success(history.OrderByDescending(h => h.TransactionDate).ToList());
    }
}
