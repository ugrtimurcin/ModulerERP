using ModulerERP.SharedKernel.Entities;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Domain.Entities;

/// <summary>
/// Employee record matching EmployeeDetails in spec.
/// Links to SystemCore.Users via UserId.
/// </summary>
public class Employee : BaseEntity
{
    // inherited: Id (mapped to User.Id or separate?), TenantId, etc.
    // Spec says "Extended profile for Users". Usually joined by Id or UserId FK.
    // Modular Monolith -> UserId FK is cleaner decoupling.

    public Guid? UserId { get; private set; } // Link to System User
    public string IdentityNumber { get; private set; } = string.Empty; // TRNC ID
    public DateTime? BirthDate { get; private set; }
    public string JobTitle { get; private set; } = string.Empty;
    public Guid DepartmentId { get; private set; }
    public Guid? SupervisorId { get; private set; }
    public string? QrToken { get; private set; } // Encrypted
    public string? Iban { get; private set; }
    public string? BankName { get; private set; }
    public decimal CurrentSalary { get; private set; }
    public Guid? SalaryCurrencyId { get; private set; }
    public decimal TransportAmount { get; private set; }
    
    // Legal & KKTC Specifics
    public CitizenshipType Citizenship { get; private set; } = CitizenshipType.TRNC; // Default to TRNC
    public SocialSecurityType SocialSecurityType { get; private set; } = SocialSecurityType.Standard;
    public MaritalStatus MaritalStatus { get; private set; } = MaritalStatus.Single;
    public bool IsSpouseWorking { get; private set; }
    public int ChildCount { get; private set; }
    public bool IsPensioner { get; private set; }

    public string? WorkPermitNumber { get; private set; }
    public DateTime? WorkPermitExpDate { get; private set; }
    
    // Redundant but helpful basics usually present in Employee tables if User doesn't exist yet
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public EmploymentStatus Status { get; private set; } = EmploymentStatus.Active;

    // Navigation
    public Department? Department { get; private set; }
    public Employee? Supervisor { get; private set; }
    
    // Collections
    public ICollection<SalaryHistory> SalaryHistory { get; private set; } = new List<SalaryHistory>();
    public ICollection<AttendanceLog> AttendanceLogs { get; private set; } = new List<AttendanceLog>();
    public ICollection<DailyAttendance> DailyAttendances { get; private set; } = new List<DailyAttendance>();
    public ICollection<LeaveRequest> LeaveRequests { get; private set; } = new List<LeaveRequest>();
    public ICollection<PeriodCommission> Commissions { get; private set; } = new List<PeriodCommission>();
    public ICollection<AdvanceRequest> AdvanceRequests { get; private set; } = new List<AdvanceRequest>();
    public ICollection<PayrollEntry> PayrollEntries { get; private set; } = new List<PayrollEntry>();

    private Employee() { }

    public static Employee Create(
        Guid tenantId,
        Guid createdBy,
        string firstName,
        string lastName,
        string email,
        Guid departmentId,
        string jobTitle,
        string identityNumber,
        decimal salary,
        decimal transportAmount = 0,
        Guid? userId = null,
        MaritalStatus maritalStatus = MaritalStatus.Single,
        bool isSpouseWorking = false,
        int childCount = 0,
        bool isPensioner = false)
    {
        var emp = new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            DepartmentId = departmentId,
            JobTitle = jobTitle,
            IdentityNumber = identityNumber,
            CurrentSalary = salary,
            TransportAmount = transportAmount,
            UserId = userId,
            MaritalStatus = maritalStatus,
            IsSpouseWorking = isSpouseWorking,
            ChildCount = childCount,
            IsPensioner = isPensioner
        };
        emp.SetTenant(tenantId);
        emp.SetCreator(createdBy);
        return emp;
    }

    public void UpdateJob(string jobTitle, Guid departmentId, Guid? supervisorId)
    {
        JobTitle = jobTitle;
        DepartmentId = departmentId;
        SupervisorId = supervisorId;
    }

    public void UpdatePersonalDetails(string firstName, string lastName, string email, EmploymentStatus status)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Status = status;
    }

    public void UpdateLegalDetails(
        CitizenshipType citizenship, 
        SocialSecurityType socialSecurityType,
        string? workPermitNumber, 
        DateTime? workPermitExpDate, 
        string? bankName, 
        string? iban,
        MaritalStatus maritalStatus,
        bool isSpouseWorking,
        int childCount,
        bool isPensioner)
    {
        Citizenship = citizenship;
        SocialSecurityType = socialSecurityType;
        WorkPermitNumber = workPermitNumber;
        WorkPermitExpDate = workPermitExpDate;
        BankName = bankName;
        Iban = iban;
        MaritalStatus = maritalStatus;
        IsSpouseWorking = isSpouseWorking;
        ChildCount = childCount;
        IsPensioner = isPensioner;
    }

    public void SetSalary(decimal amount, Guid? currencyId, decimal transportAmount = 0)
    {
        CurrentSalary = amount;
        SalaryCurrencyId = currencyId;
        TransportAmount = transportAmount;
        // Should trigger history update in Service
    }

    public void SetQrToken(string token) => QrToken = token;
}
