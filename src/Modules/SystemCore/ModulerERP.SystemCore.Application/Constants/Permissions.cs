namespace ModulerERP.SystemCore.Application.Constants;

public static class Permissions
{
    public static class Users
    {
        public const string View = "users.view";
        public const string Create = "users.create";
        public const string Edit = "users.edit";
        public const string Delete = "users.delete";
    }

    public static class Roles
    {
        public const string View = "roles.view";
        public const string Create = "roles.create";
        public const string Edit = "roles.edit";
        public const string Delete = "roles.delete";
    }

    public static class Tenants
    {
        public const string View = "tenants.view";
        public const string Register = "tenants.register";
        public const string Manage = "tenants.manage";
    }
    
    public static IEnumerable<string> GetAll()
    {
        return new List<string>
        {
            Users.View, Users.Create, Users.Edit, Users.Delete,
            Roles.View, Roles.Create, Roles.Edit, Roles.Delete,
            Tenants.View, Tenants.Register
        };
    }
}
