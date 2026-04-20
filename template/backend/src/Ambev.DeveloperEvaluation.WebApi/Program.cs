using Ambev.DeveloperEvaluation.Application;
using MassTransit;
using Ambev.DeveloperEvaluation.Common.HealthChecks;
using Ambev.DeveloperEvaluation.Common.Logging;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.IoC;
using Ambev.DeveloperEvaluation.ORM;
using StackExchange.Redis;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

using Ambev.DeveloperEvaluation.WebApi.Middleware;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Ambev.DeveloperEvaluation.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Log.Information("Starting web application");

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.AddDefaultLogging();

            builder.Services.AddControllers();

            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("developer");
                        h.Password("ev@luAt10n");
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });
            builder.Services.AddEndpointsApiExplorer();

            builder.AddBasicHealthChecks();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<DefaultContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM")
                )
            );

            builder.Services.AddJwtAuthentication(builder.Configuration);

            builder.RegisterDependencies();

            builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(ApplicationLayer).Assembly);

            // =======================================
            // Observability/Tracing - OpenTelemetry
            // =======================================
            // Esta seção ativa a instrumentação automática de requests HTTP, chamadas HTTP client, Entity Framework Core e Redis.
            // Todos os eventos/traces são exportados para o console (localmente), permitindo "ver" fluxos de requisições e dependências.
            //
            // Você pode depois trocar/exportar para outros destinos (Jaeger, Zipkin, Grafana, Azure Application Insights), mas o Console é ótimo para testes e didática.
            builder.Services.AddOpenTelemetry()
                .WithTracing(builder => builder
                    // Instrumentação automática de requisições nas APIs ASP.NET Core
                    .AddAspNetCoreInstrumentation()
                    // Instrumentação automática de chamadas HTTP (HttpClient)
                    .AddHttpClientInstrumentation()
                    // Instrumentação automática de banco de dados (Entity Framework Core)
                    // .AddEntityFrameworkCoreInstrumentation() // descomentar ao instalar OpenTelemetry.Instrumentation.EntityFrameworkCore
                    // Instrumentação StackExchange.Redis (instrumentação nativa precisa do pacote, aqui só exemplificando)
                    // .AddRedisInstrumentation() // descomentar se/ao instalar pacote OpenTelemetry.Instrumentation.StackExchangeRedis
                    // Exportador simples de spans para o console
                    .AddConsoleExporter()
                );
            // =======================================

// Redis registration
var redisConfig = builder.Configuration.GetSection("Redis:ConnectionString").Value;
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig));
builder.Services.AddSingleton<Ambev.DeveloperEvaluation.WebApi.Common.RedisCacheService>();


            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(
                    typeof(ApplicationLayer).Assembly,
                    typeof(Program).Assembly
                );
            });

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));



            var app = builder.Build();
            app.UseMiddleware<ValidationExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseBasicHealthChecks();

            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
