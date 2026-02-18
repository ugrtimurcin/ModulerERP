namespace ModulerERP.HR.Domain.Enums;

public enum SocialSecurityType
{
    Standard = 1,
    Pensioner = 2, // Emekli (Usually lower or different rates)
    Student = 3,   // Ogrenci
    Exempt = 4,    // Muaf
    Foreigner = 5, // Yabanci Uyruklu (Sometimes differs, though often tied to Citizenship)
    PartTime = 6   // Kismi Zamanli
}
