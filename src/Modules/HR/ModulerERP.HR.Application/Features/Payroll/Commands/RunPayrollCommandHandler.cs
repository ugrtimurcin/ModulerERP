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
    private readonly IRepository<MinimumWage> _minimumWageRepository;
    private readonly IRepository<PayrollParameter> _payrollParameterRepository;
    private readonly IRepository<EmployeeCumulative> _employeeCumulativeRepository;
    private readonly IRepository<EarningDeductionType> _earningDeductionTypeRepository;
    private readonly IHRUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RunPayrollCommandHandler(
        IRepository<ModulerERP.HR.Domain.Entities.Payroll> payrollRepository,
        IRepository<PayrollEntry> payrollEntryRepository,
        IRepository<Employee> employeeRepository,
        IRepository<TaxRule> taxRuleRepository,
        IRepository<SocialSecurityRule> socialSecurityRuleRepository,
        IRepository<DailyAttendance> dailyAttendanceRepository,
        IRepository<MinimumWage> minimumWageRepository,
        IRepository<PayrollParameter> payrollParameterRepository,
        IRepository<EmployeeCumulative> employeeCumulativeRepository,
        IRepository<EarningDeductionType> earningDeductionTypeRepository,
        IHRUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _payrollRepository = payrollRepository;
        _payrollEntryRepository = payrollEntryRepository;
        _employeeRepository = employeeRepository;
        _taxRuleRepository = taxRuleRepository;
        _socialSecurityRuleRepository = socialSecurityRuleRepository;
        _dailyAttendanceRepository = dailyAttendanceRepository;
        _minimumWageRepository = minimumWageRepository;
        _payrollParameterRepository = payrollParameterRepository;
        _employeeCumulativeRepository = employeeCumulativeRepository;
        _earningDeductionTypeRepository = earningDeductionTypeRepository;
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
            // 1. Fetch YTD Cumulative Ledger
            var ledgerList = await _employeeCumulativeRepository.FindAsync(c => c.EmployeeId == emp.Id && c.Year == dto.Year, cancellationToken);
            var ledger = ledgerList.FirstOrDefault();
            
            // If it doesn't exist, create it (new employee at month start, etc.)
            var ytdTaxBase = 0m;
            if (ledger == null)
            {
                ledger = EmployeeCumulative.Create(tenantId, _currentUserService.UserId, emp.Id, dto.Year);
                await _employeeCumulativeRepository.AddAsync(ledger, cancellationToken);
            }
            else
            {
                ytdTaxBase = ledger.YtdTaxBase;
            }

            // 2. Fetch specific Risk Profile if exists
            var riskProfile = emp.SgkRiskProfile ?? emp.Department?.SgkRiskProfile;

            // 3. Build dynamic earnings and deductions (Usually read from Timesheets/Advances mapped to EarningDeductionType)
            // For now, simulate an empty list due to refactoring. A "Payroll Generation Pipeline" module handles filling this.
            var earningsAndDeductions = new List<PayrollEntryDetail>();

            // Find appropriate SS rule
            var ssRule = ssRules.FirstOrDefault(r => r.CitizenshipType == emp.Citizenship && r.SocialSecurityType == emp.SocialSecurityType) 
                         ?? ssRules.FirstOrDefault(r => r.CitizenshipType == CitizenshipType.TRNC && r.SocialSecurityType == SocialSecurityType.Standard);

            var calc = PayrollCalculator.Calculate(
                emp, 
                emp.CurrentSalary, 
                ytdTaxBase,
                earningsAndDeductions,
                taxRules,
                riskProfile,
                ssRule,
                minWage,
                parameters);

            // Update Ledger with new tax base
            // In TRNC: Current taxable before deductions minus personal allowance
            var monthlyTaxBase = (calc.TotalTaxableEarnings + emp.CurrentSalary) - (calc.SocialSecurityEmployee + calc.ProvidentFundEmployee + calc.UnemploymentInsuranceEmployee) - calc.PersonalAllowanceDeduction;
            if (monthlyTaxBase > 0)
            {
                ledger.AddMonthlyTaxBase(monthlyTaxBase);
            }

            var entry = PayrollEntry.Create(
                tenantId,
                _currentUserService.UserId,
                payroll.Id,
                emp.Id,
                emp.CurrentSalary, 
                calc.CumulativeTaxBaseBeforeThisPayroll,
                calc.TotalTaxableEarnings,
                calc.TotalSgkExemptEarnings,
                calc.SocialSecurityEmployee,
                calc.ProvidentFundEmployee,
                calc.UnemploymentInsuranceEmployee,
                calc.PersonalAllowanceDeduction,
                calc.IncomeTax,
                calc.StampTax,
                calc.SocialSecurityEmployer,
                calc.ProvidentFundEmployer,
                calc.UnemploymentInsuranceEmployer,
                calc.NetPayable,
                1.0m // exchange rate
            );

            await _payrollEntryRepository.AddAsync(entry, cancellationToken);
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
