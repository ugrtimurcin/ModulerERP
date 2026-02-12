using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly IRepository<Payroll> _payrollRepository;
    private readonly IRepository<PayrollEntry> _entryRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public PayrollService(
        IRepository<Payroll> payrollRepository,
        IRepository<PayrollEntry> entryRepository,
        IRepository<Employee> employeeRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _payrollRepository = payrollRepository;
        _entryRepository = entryRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<PayrollDto>> GetByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUserService.TenantId;
        var yearPrefix = $"{year}-";
        
        var payrolls = await _payrollRepository.FindAsync(
            p => p.TenantId == tenantId && p.Period.StartsWith(yearPrefix),
            cancellationToken);

        // Get entry counts per payroll
        var payrollIds = payrolls.Select(p => p.Id).ToList();
        var entries = await _entryRepository.FindAsync(e => payrollIds.Contains(e.PayrollId), cancellationToken);
        var entryCounts = entries.GroupBy(e => e.PayrollId).ToDictionary(g => g.Key, g => g.Count());

        return payrolls.Select(p => new PayrollDto(
            p.Id,
            p.Period,
            p.Description,
            p.Status,
            p.TotalAmount,
            p.CurrencyId,
            "TRY", // Default currency code - ideally lookup
            entryCounts.GetValueOrDefault(p.Id, 0),
            p.CreatedAt
        )).OrderByDescending(p => p.Period);
    }

    public async Task<PayrollDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payroll = await _payrollRepository.GetByIdAsync(id, cancellationToken);
        if (payroll == null) return null;

        var entries = await _entryRepository.FindAsync(e => e.PayrollId == id, cancellationToken);

        return new PayrollDto(
            payroll.Id,
            payroll.Period,
            payroll.Description,
            payroll.Status,
            payroll.TotalAmount,
            payroll.CurrencyId,
            "TRY",
            entries.Count(),
            payroll.CreatedAt
        );
    }

    public async Task<IEnumerable<PayrollEntryDto>> GetEntriesAsync(Guid payrollId, CancellationToken cancellationToken = default)
    {
        var entries = await _entryRepository.FindAsync(e => e.PayrollId == payrollId, cancellationToken);

        var employeeIds = entries.Select(e => e.EmployeeId).Distinct().ToList();
        var employees = (await _employeeRepository.FindAsync(e => employeeIds.Contains(e.Id), cancellationToken))
                        .ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}");

        return entries.Select(e => new PayrollEntryDto(
            e.Id,
            e.PayrollId,
            e.EmployeeId,
            employees.GetValueOrDefault(e.EmployeeId, "Unknown"),
            e.BaseSalary,
            e.OvertimePay,
            e.CommissionPay,
            e.AdvanceDeduction,
            e.SocialSecurityEmployee,
            e.ProvidentFundEmployee,
            e.IncomeTax,
            e.NetPayable
        ));
    }

    public async Task<PayrollDto> RunPayrollAsync(RunPayrollDto dto, CancellationToken cancellationToken = default)
    {
        var period = $"{dto.Year}-{dto.Month:D2}";
        var tenantId = _currentUserService.TenantId;

        // Check if payroll already exists for this period
        var existing = await _payrollRepository.FindAsync(
            p => p.TenantId == tenantId && p.Period == period,
            cancellationToken);

        if (existing.Any())
            throw new InvalidOperationException($"Payroll for period {period} already exists.");

        // Get all active employees
        var employees = await _employeeRepository.FindAsync(
            e => e.TenantId == tenantId && e.Status == EmploymentStatus.Active,
            cancellationToken);

        // Create payroll record
        var currencyId = dto.CurrencyId ?? Guid.Empty; // Default currency
        var payroll = Payroll.Create(
            tenantId,
            _currentUserService.UserId,
            period,
            $"Payroll for {period}",
            currencyId
        );

        await _payrollRepository.AddAsync(payroll, cancellationToken);

        // Create entries for each employee
        foreach (var emp in employees)
        {
            var calc = PayrollCalculator.Calculate(emp, emp.CurrentSalary, 0, 0, 0, 0); // TODO: fetch bonus/overtime/advance from repositories

            var entry = PayrollEntry.Create(
                tenantId,
                _currentUserService.UserId,
                payroll.Id,
                emp.Id,
                calc.GrossSalary, // Using Gross as Base for now, should split if bonus exists
                0, // overtime (included in gross calc but separate field here?)
                0, // commission
                0, // bonus
                0, // transport
                calc.SocialSecurityEmployee,
                calc.ProvidentFundEmployee,
                calc.IncomeTax,
                calc.AdvanceDeduction,
                calc.SocialSecurityEmployer,
                calc.ProvidentFundEmployer,
                calc.UnemploymentInsuranceEmployer,
                calc.NetPayable,
                1.0m // exchange rate
            );

            payroll.AddEntry(entry);
            await _entryRepository.AddAsync(entry, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(payroll.Id, cancellationToken))!;
    }

    public async Task<PayrollSummaryDto> GetSummaryAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var period = $"{year}-{month:D2}";
        var tenantId = _currentUserService.TenantId;

        var payrolls = await _payrollRepository.FindAsync(
            p => p.TenantId == tenantId && p.Period == period,
            cancellationToken);

        var payroll = payrolls.FirstOrDefault();
        if (payroll == null)
        {
            return new PayrollSummaryDto(year, month, 0, 0, 0, 0, "TRY");
        }

        var entries = await _entryRepository.FindAsync(e => e.PayrollId == payroll.Id, cancellationToken);
        var totalGross = entries.Sum(e => e.BaseSalary + e.OvertimePay + e.CommissionPay);
        var totalDeductions = entries.Sum(e => e.IncomeTax + e.AdvanceDeduction);
        var totalNet = entries.Sum(e => e.NetPayable);

        return new PayrollSummaryDto(
            year,
            month,
            totalGross,
            totalDeductions,
            totalNet,
            entries.Count(),
            "TRY"
        );
    }
}
