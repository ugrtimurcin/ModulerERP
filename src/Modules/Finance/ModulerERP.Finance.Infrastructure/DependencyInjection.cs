using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ModulerERP.Finance.Infrastructure.Persistence;
using ModulerERP.Finance.Application.Services;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;

namespace ModulerERP.Finance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FinanceDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(FinanceDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<FinanceDbContext>());
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
        
        // Register the Shared Interface Implementation
        services.AddScoped<IFinanceOperationsService, FinanceOperationsService>();
        services.AddScoped<IJournalEntryService, JournalEntryService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IFiscalPeriodService, FiscalPeriodService>();
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        services.AddHttpClient<ModulerERP.Finance.Application.Interfaces.IExchangeRateProvider, ModulerERP.Finance.Infrastructure.Services.KktcRateProvider>();
        
        // Register Generic Repositories for Finance Entities
        services.AddScoped<IRepository<FiscalPeriod>, FinanceRepository<FiscalPeriod>>();
        services.AddScoped<IRepository<Account>, FinanceRepository<Account>>();
        services.AddScoped<IRepository<Payment>, FinanceRepository<Payment>>();
        services.AddScoped<IRepository<ExchangeRate>, FinanceRepository<ExchangeRate>>();
        services.AddScoped<IRepository<Cheque>, FinanceRepository<Cheque>>();
        services.AddScoped<IRepository<ChequeHistory>, FinanceRepository<ChequeHistory>>();
        services.AddScoped<IChequeService, ChequeService>();

        return services;
    }
}
