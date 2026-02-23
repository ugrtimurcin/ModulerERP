using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class HrSetting : BaseEntity
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private HrSetting() { }

    public static HrSetting Create(Guid tenantId, Guid createdBy, string key, string value, string description)
    {
        var setting = new HrSetting
        {
            Key = key,
            Value = value,
            Description = description
        };
        setting.SetTenant(tenantId);
        setting.SetCreator(createdBy);
        return setting;
    }

    public void Update(string value, string description)
    {
        Value = value;
        Description = description;
    }
}
