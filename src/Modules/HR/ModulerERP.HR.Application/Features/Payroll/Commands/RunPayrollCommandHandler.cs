using MediatR;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Application.Services; // For PayrollCalculator
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Features.Payroll.Commands;

public class RunPayrollCommandHandler : IRequestHandler<RunPayrollCommand, PayrollDto>
{
    private readonly IRepository<ModulerERP.HR.Domain.Entities.Payroll> _payrollRepository;
    private readonly IRepository<PayrollEntry> _payrollEntryRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<TaxRule> _taxRuleRepository;
    private readonly IRepository<SocialSecurityRule> _socialSecurityRuleRepository;
    private readonly IRepository<DailyAttendance> _dailyAttendanceRepository;
    private readonly IRepository<PeriodCommission> _commissionRepository;
    private readonly IRepository<AdvanceRequest> _advanceRepository;
    private readonly IRepository<Bonus> _bonusRepository;
    private readonly IRepository<MinimumWage> _minimumWageRepository; // Added
    private readonly IRepository<PayrollParameter> _payrollParameterRepository; // Added
    private readonly IHRUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RunPayrollCommandHandler(
        IRepository<ModulerERP.HR.Domain.Entities.Payroll> payrollRepository,
        IRepository<PayrollEntry> payrollEntryRepository,
        IRepository<Employee> employeeRepository,
        IRepository<TaxRule> taxRuleRepository,
        IRepository<SocialSecurityRule> socialSecurityRuleRepository,
        IRepository<DailyAttendance> dailyAttendanceRepository,
        IRepository<PeriodCommission> commissionRepository,
        IRepository<AdvanceRequest> advanceRepository,
        IRepository<Bonus> bonusRepository,
        IRepository<MinimumWage> minimumWageRepository, // Added
        IRepository<PayrollParameter> payrollParameterRepository, // Added
        IHRUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _payrollRepository = payrollRepository;
        _payrollEntryRepository = payrollEntryRepository;
        _employeeRepository = employeeRepository;
        _taxRuleRepository = taxRuleRepository;
        _socialSecurityRuleRepository = socialSecurityRuleRepository;
        _dailyAttendanceRepository = dailyAttendanceRepository;
        _commissionRepository = commissionRepository;
        _advanceRepository = advanceRepository;
        _bonusRepository = bonusRepository;
        _minimumWageRepository = minimumWageRepository;
        _payrollParameterRepository = payrollParameterRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PayrollDto> Handle(RunPayrollCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var period = $"{dto.Year}-{dto.Month:D2}";
        var tenantId = _currentUserService.TenantId;
        var periodDate = new DateTime(dto.Year, dto.Month, 1);
        var periodEnd = periodDate.AddMonths(1).AddDays(-1);

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

        // Fetch Rules
        var taxRules = await _taxRuleRepository.FindAsync(
            r => r.EffectiveFrom <= periodDate && (r.EffectiveTo == null || r.EffectiveTo >= periodDate), 
            cancellationToken);
            
        var ssRules = await _socialSecurityRuleRepository.FindAsync(
            r => r.EffectiveFrom <= periodDate && (r.EffectiveTo == null || r.EffectiveTo >= periodDate), 
            cancellationToken);

        // Fetch Minimum Wage
        var minWages = await _minimumWageRepository.FindAsync(
            m => m.TenantId == tenantId && m.EffectiveFrom <= periodDate && (m.EffectiveTo == null || m.EffectiveTo >= periodDate),
            cancellationToken);
        var minWage = minWages.OrderByDescending(m => m.EffectiveFrom).FirstOrDefault();

        // Fetch Payroll Parameters
        var parameters = await _payrollParameterRepository.FindAsync(
            p => p.TenantId == tenantId,
            cancellationToken);

        // Create payroll record
        var currencyId = dto.CurrencyId ?? Guid.Empty; // Default currency
        var payroll = ModulerERP.HR.Domain.Entities.Payroll.Create(
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
            // 1. Fetch Overtime
            var attendances = await _dailyAttendanceRepository.FindAsync(
                a => a.EmployeeId == emp.Id && a.Date >= periodDate && a.Date <= periodEnd, 
                cancellationToken);
            
            var ot1Mins = attendances.Sum(a => a.Overtime1xMins);
            var ot2Mins = attendances.Sum(a => a.Overtime2xMins);
            
            // Standard Rate: Salary / 240 hours
            var hourlyRate = emp.CurrentSalary / 240m; 
            var ot1Pay = (ot1Mins / 60m) * hourlyRate * 1.5m; // Weekday Overtime (1.5x)
            var ot2Pay = (ot2Mins / 60m) * hourlyRate * 2.0m; // Weekend/Holiday Overtime (2.0x)
            var overtimePay = ot1Pay + ot2Pay;

            // 2. Fetch Commissions
            var commissions = await _commissionRepository.FindAsync(
                c => c.EmployeeId == emp.Id && c.Period == period, // Use Period string matching Commission Period
                cancellationToken);
            var commissionPay = commissions.Sum(c => c.FinalAmount);

            // 3. Fetch Advances
            // Approved, Paid, Not Deducted, and due inside this period (or overdue?)
            var advances = await _advanceRepository.FindAsync(
                a => a.EmployeeId == emp.Id 
                     && a.IsPaid 
                     && !a.IsDeducted 
                     && (a.RepaymentDate == null || (a.RepaymentDate >= periodDate && a.RepaymentDate <= periodEnd)), 
                cancellationToken);
            var advanceDeduction = advances.Sum(a => a.Amount);

            // 4. Fetch Bonuses
            var bonuses = await _bonusRepository.FindAsync(
                b => b.EmployeeId == emp.Id && b.Period == period && !b.IsProcessed,
                cancellationToken);
            var bonusPay = bonuses.Sum(b => b.Amount);
            
            // Mark bonuses as processed
            foreach(var b in bonuses)
            {
                b.MarkAsProcessed();
            }

            // Find appropriate SS rule
            var ssRule = ssRules.FirstOrDefault(r => r.CitizenshipType == emp.Citizenship && r.SocialSecurityType == emp.SocialSecurityType) 
                         ?? ssRules.FirstOrDefault(r => r.CitizenshipType == CitizenshipType.TRNC && r.SocialSecurityType == SocialSecurityType.Standard);

            var calc = PayrollCalculator.Calculate(
                emp, 
                emp.CurrentSalary, 
                bonusPay, 
                overtimePay, 
                commissionPay, 
                advanceDeduction,
                emp.TransportAmount,
                taxRules,
                ssRule,
                minWage,
                parameters);

            var entry = PayrollEntry.Create(
                tenantId,
                _currentUserService.UserId,
                payroll.Id,
                emp.Id,
                emp.CurrentSalary, // Fixed: Pass Base Salary, not Total Gross
                overtimePay, 
                commissionPay, 
                bonusPay, 
                emp.TransportAmount,
                calc.SocialSecurityEmployee,
                calc.ProvidentFundEmployee,
                calc.UnemploymentInsuranceEmployee,
                calc.IncomeTax,
                calc.AdvanceDeduction,
                calc.SocialSecurityEmployer,
                calc.ProvidentFundEmployer,
                calc.UnemploymentInsuranceEmployer,
                calc.NetPayable,
                1.0m // exchange rate
            );

            await _payrollEntryRepository.AddAsync(entry, cancellationToken);
            
            // Mark advances as deducted (Tracked entities will update on SaveChanges)
            foreach(var adv in advances) 
            { 
                adv.MarkAsDeducted(); 
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch updated entires count if needed, or simply return DTO
        var entryCount = employees.Count();

        return new PayrollDto(
            payroll.Id,
            payroll.Period,
            payroll.Description,
            payroll.Status,
            payroll.TotalAmount,
            payroll.CurrencyId,
            "TRY", // Placeholder for now
            entryCount,
            payroll.CreatedAt
        );
    }
}
