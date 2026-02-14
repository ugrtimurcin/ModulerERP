using System.Reflection;

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

    public static class Inventory
    {
        public const string View = "inventory.view";
        public const string ManageProducts = "inventory.products.manage";
        public const string StockAdjust = "inventory.stock.adjust";
        public const string Transfers = "inventory.transfers.manage";
    }

    public static class HR
    {
        public const string View = "hr.view";
        public const string ManageEmployees = "hr.employees.manage";
        public const string ManagePayroll = "hr.payroll.manage";
        public const string ManageAttendance = "hr.attendance.manage";
    }

    public static class CRM
    {
        public const string View = "crm.view";
        public const string ManagePartners = "crm.partners.manage";
        public const string ManageLeads = "crm.leads.manage";
        public const string ManageOpportunities = "crm.opportunities.manage";
    }

    public static class Finance
    {
        public const string View = "finance.view";
        public const string ManageAccounts = "finance.accounts.manage";
        public const string ManageTransactions = "finance.transactions.manage";
        public const string ManageExchangeRates = "finance.exchangerates.manage";
    }

    public static class Projects
    {
        public const string View = "projects.view";
        public const string Manage = "projects.manage";
        public const string ApproveLogs = "projects.logs.approve";
    }

    public static class Procurement
    {
        public const string View = "procurement.view";
        public const string ManageRfqs = "procurement.rfqs.manage";
        public const string ManageOrders = "procurement.orders.manage";
    }

    public static class Sales
    {
        public const string View = "sales.view";
        public const string ManageOrders = "sales.orders.manage";
        public const string ManageInvoices = "sales.invoices.manage";
    }

    public static class FixedAssets
    {
        public const string View = "fixedassets.view";
        public const string Manage = "fixedassets.manage";
    }

    public static class Manufacturing
    {
        public const string View = "manufacturing.view";
        public const string Manage = "manufacturing.manage";
    }

    public static IEnumerable<string> GetAll()
    {
        var permissions = new List<string>();
        var nestedTypes = typeof(Permissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        foreach (var type in nestedTypes)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (var field in fields)
            {
                var value = field.GetValue(null);
                if (value is string permission)
                {
                    permissions.Add(permission);
                }
            }
        }

        return permissions;
    }
}
