using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Application.Services;

public class AdvanceRequestService : IAdvanceRequestService
{
    private readonly IRepository<AdvanceRequest> _repository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AdvanceRequestService(
        IRepository<AdvanceRequest> repository,
        IRepository<Employee> employeeRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<AdvanceRequestDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _repository.GetAllAsync(cancellationToken);
        
        // Optimize: Use Spec with Include in future
        var employees = (await _employeeRepository.GetAllAsync(cancellationToken)).ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}");

        return requests.Select(r => ToDto(r, employees.GetValueOrDefault(r.EmployeeId, "Unknown")));
    }

    public async Task<IEnumerable<AdvanceRequestDto>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        // TODO: Use specification
        var all = await GetAllAsync(cancellationToken);
        return all.Where(x => x.EmployeeId == employeeId);
    }

    public async Task<AdvanceRequestDto> CreateAsync(CreateAdvanceRequestDto dto, CancellationToken cancellationToken = default)
    {
        var emp = await _employeeRepository.GetByIdAsync(dto.EmployeeId, cancellationToken);
        if (emp == null) throw new KeyNotFoundException("Employee not found");

        var entity = AdvanceRequest.Create(
            _currentUserService.TenantId,
            _currentUserService.UserId,
            dto.EmployeeId,
            dto.Amount,
            dto.Description
        );

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(entity, $"{emp.FirstName} {emp.LastName}");
    }

    public async Task ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) throw new KeyNotFoundException();
        
        entity.SetStatus(AdvanceRequestStatus.Approved);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) throw new KeyNotFoundException();

        entity.SetStatus(AdvanceRequestStatus.Rejected);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsPaidAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) throw new KeyNotFoundException();

        entity.SetStatus(AdvanceRequestStatus.Paid);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static AdvanceRequestDto ToDto(AdvanceRequest r, string empName) => new(
        r.Id,
        r.EmployeeId,
        empName,
        r.RequestDate,
        r.Amount,
        (int)r.Status,
        r.Status.ToString(),
        r.Description
    );
}
