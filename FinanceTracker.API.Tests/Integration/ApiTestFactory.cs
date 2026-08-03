using FinanceTracker.API.Data;
using FinanceTracker.API.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceTracker.API.Tests.Integration
{
    // Reemplaza SQL Server por una base InMemory por cada instancia de la factory.
    //
    // Nota: NO se puede sobreescribir JwtSettings vía ConfigureAppConfiguration acá.
    // Program.cs lee "JwtSettings" en una variable local ANTES de que WebApplicationFactory
    // pueda inyectar su propia configuración (esa inyección ocurre recién al interceptar
    // builder.Build(), más tarde en la ejecución de Program.cs). Si se sobreescribe JwtSettings
    // acá, JwtBearerOptions valida contra la clave real de appsettings.Development.json mientras
    // que AuthService firma con la clave de test -> todo token generado en un test da 401.
    // Por eso se usa el entorno Development tal cual, con la clave de appsettings.Development.json.
    public class ApiTestFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                // EF Core (10.x) acumula configuraciones vía IDbContextOptionsConfiguration<T>
                // en vez de reemplazar el descriptor de DbContextOptions<T>: hay que sacar
                // toda referencia a FinanceTrackerContext (no solo DbContextOptions<T>),
                // si no, la config de UseSqlServer original se sigue aplicando junto con UseInMemoryDatabase.
                var descriptorsToRemove = services
                    .Where(d =>
                        d.ServiceType == typeof(FinanceTrackerContext) ||
                        (d.ServiceType.IsGenericType && d.ServiceType.GetGenericArguments().Contains(typeof(FinanceTrackerContext))))
                    .ToList();
                foreach (var descriptor in descriptorsToRemove)
                    services.Remove(descriptor);

                services.AddDbContext<FinanceTrackerContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                    options.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });

                using var scope = services.BuildServiceProvider().CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FinanceTrackerContext>();
                context.Database.EnsureCreated();
                SeedReferenceData(context);
            });
        }

        private static void SeedReferenceData(FinanceTrackerContext context)
        {
            if (!context.Roles.Any())
                context.Roles.Add(new Role { Name = "USER" });

            if (!context.Currencies.Any())
            {
                context.Currencies.Add(new Currency { Code = "UYU", Name = "Peso uruguayo", Symbol = "$" });
                context.Currencies.Add(new Currency { Code = "USD", Name = "Dólar", Symbol = "US$" });
            }

            context.SaveChanges();
        }
    }
}
