using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Application.DTOs;

public record PayrollDto(
    Guid Id,
    string Period,
    string Description,
    PayrollStatus Status,
    decimal TotalAmount,
    Guid CurrencyId,
    string CurrencyCode,
    int EmployeeCount,
    DateTime CreatedAt
);

public record PayrollEntryDto(
    Guid Id,
    Guid PayrollId,
    Guid EmployeeId,
    string EmployeeName,
    decimal BaseSalary,
    decimal OvertimePay,
    decimal CommissionPay,
    decimal BonusPay,
    decimal TransportAmount,
    decimal AdvanceDeduction,
    decimal SocialSecurityEmployee,
    decimal ProvidentFundEmployee,
    decimal IncomeTax,
    decimal NetPayable
);

public record RunPayrollDto(
    int Year,
    int Month,
    Guid? CurrencyId
);

public record PayrollSummaryDto(
    int Year,
    int Month,
    decimal TotalGross,
    decimal TotalDeductions,
    decimal TotalNet,
    int EmployeeCount,
    string CurrencyCode
);
