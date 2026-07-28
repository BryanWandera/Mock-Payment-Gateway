using System.Text.Json.Serialization;
using Dapper;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using PaymentMock.Configuration;
using PaymentMock.Database;
using PaymentMock.Enums;
using PaymentMock.Extensions;
using PaymentMock.Repositories.Implementations;
using PaymentMock.Repositories.Interfaces;
using PaymentMock.Services.Background;
using PaymentMock.Services.Implementations;
using PaymentMock.Services.Implementations.Profiles;
using PaymentMock.Services.Interfaces;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/paymentmock-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    var appSettings = new AppSettings();
    builder.Configuration.Bind(appSettings);
    builder.Services.Configure<AppSettings>(builder.Configuration);

    RegisterDapperTypeHandlers();
    RegisterRepositories(builder.Services);
    RegisterServices(builder.Services, appSettings);

    builder.Services.AddHttpClient("webhooks", c => c.Timeout = TimeSpan.FromSeconds(10));

    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true)
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Mock Payment Gateway",
            Version = "v1",
            Description = "A provider-agnostic mock payment gateway for end-to-end payment integration testing."
        });

        c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Name = "X-API-Key",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Description = "API key required for the Generic gateway profile. Example: X-API-Key: {your-key}"
        });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Bearer token required for the Pesapal gateway profile. Obtain one from /api/v1/auth/token"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" } },
                Array.Empty<string>()
            },
            {
                new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddHealthChecks()
        .AddMySql(appSettings.Database.ConnectionString, name: "mysql");

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        await DbInitializer.InitializeAsync(appSettings.Database.ConnectionString, logger);

        var configurationService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
        await configurationService.RecordSnapshotAsync("Startup", configurationService.GetEffectiveConfiguration());
    }

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mock Payment Gateway v1"));

    app.UsePaymentMockMiddleware();

    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Mock Payment Gateway starting with profile {Profile}", appSettings.GatewayProfile);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Mock Payment Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static void RegisterDapperTypeHandlers()
{
    SqlMapper.AddTypeHandler(new EnumTypeHandler<TransactionStatus>());
    SqlMapper.AddTypeHandler(new EnumTypeHandler<PayoutStatus>());
    SqlMapper.AddTypeHandler(new EnumTypeHandler<PaymentProvider>());
    SqlMapper.AddTypeHandler(new EnumTypeHandler<PaymentMethod>());
    SqlMapper.AddTypeHandler(new EnumTypeHandler<WebhookDeliveryStatus>());
}

static void RegisterRepositories(IServiceCollection services)
{
    services.AddSingleton<ITransactionRepository>(sp =>
        new TransactionRepository(ConnectionString(sp)));
    services.AddSingleton<ITransactionStateHistoryRepository>(sp =>
        new TransactionStateHistoryRepository(ConnectionString(sp)));
    services.AddSingleton<IPayoutRepository>(sp =>
        new PayoutRepository(ConnectionString(sp)));
    services.AddSingleton<IPayoutStateHistoryRepository>(sp =>
        new PayoutStateHistoryRepository(ConnectionString(sp)));
    services.AddSingleton<IWebhookDeliveryRepository>(sp =>
        new WebhookDeliveryRepository(ConnectionString(sp)));
    services.AddSingleton<IWebhookRegistrationRepository>(sp =>
        new WebhookRegistrationRepository(ConnectionString(sp)));
    services.AddSingleton<IIdempotencyRepository>(sp =>
        new IdempotencyRepository(ConnectionString(sp)));
    services.AddSingleton<IGatewayAccountRepository>(sp =>
        new GatewayAccountRepository(ConnectionString(sp)));
    services.AddSingleton<IScenarioExecutionRepository>(sp =>
        new ScenarioExecutionRepository(ConnectionString(sp)));
    services.AddSingleton<IAuditLogRepository>(sp =>
        new AuditLogRepository(ConnectionString(sp)));
    services.AddSingleton<IConfigHistoryRepository>(sp =>
        new ConfigHistoryRepository(ConnectionString(sp)));
}

static void RegisterServices(IServiceCollection services, AppSettings appSettings)
{
    services.AddSingleton<ITransactionProcessingQueue, TransactionProcessingQueue>();
    services.AddSingleton<IPayoutProcessingQueue, PayoutProcessingQueue>();

    services.AddSingleton<IValidationService, ValidationService>();
    services.AddSingleton<IScenarioEngine, ScenarioEngine>();
    services.AddSingleton<IAuthenticationService, AuthenticationService>();
    services.AddSingleton<IWebhookService, WebhookService>();
    services.AddSingleton<IPaymentService, PaymentService>();
    services.AddSingleton<ICheckoutService, CheckoutService>();
    services.AddSingleton<ITransactionService, TransactionService>();
    services.AddSingleton<IPayoutService, PayoutService>();
    services.AddSingleton<IGatewayAccountService, GatewayAccountService>();
    services.AddSingleton<ILoggingQueryService, LoggingQueryService>();
    services.AddSingleton<IConfigurationService, ConfigurationService>();

    services.AddSingleton<IGatewayProfileStrategy>(_ =>
        Enum.TryParse<GatewayProfileType>(appSettings.GatewayProfile, true, out var profile) && profile == GatewayProfileType.Pesapal
            ? new PesapalGatewayProfileStrategy()
            : new GenericGatewayProfileStrategy());

    services.AddHostedService<TransactionProcessingBackgroundService>();
    services.AddHostedService<PayoutProcessingBackgroundService>();
    services.AddHostedService<WebhookDeliveryBackgroundService>();
}

static string ConnectionString(IServiceProvider sp) =>
    sp.GetRequiredService<IOptions<AppSettings>>().Value.Database.ConnectionString;
