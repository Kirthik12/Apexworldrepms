using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using ApexWorld_Backend.Data;
using ApexWorld_Backend.Common.Models;
using ApexWorld_Backend.Features.Property.Models;
using ApexWorld_Backend.Features.Booking.Models;
using ApexWorld_Backend.Features.Loan.Models;
using ApexWorld_Backend.Features.Review.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Hangfire;
using Polly;
using Swashbuckle.AspNetCore.SwaggerGen;
using ApexWorld_Backend.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using ApexWorld_Backend.Features.AiCompanion.Plugins;

// Load .env file variables into environment
DotNetEnv.Env.Load("secret.env");

var builder = WebApplication.CreateBuilder(args);

// Register Rule Engine
builder.Services.AddRuleEngine();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Add services to the container.

// Options Pattern Setup
var jwtSettingsSection = builder.Configuration.GetSection(JwtSettings.SectionName);
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

// DbContext Setup (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<ApplicationReadOnlyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ReadReplicaConnection") ?? builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories & Services
builder.Services.AddScoped(typeof(ApexWorld_Backend.Common.Interfaces.IRepository<>), typeof(ApexWorld_Backend.Data.Repositories.Repository<>));
builder.Services.AddScoped(typeof(ApexWorld_Backend.Common.Interfaces.IReadOnlyRepository<>), typeof(ApexWorld_Backend.Data.Repositories.ReadOnlyRepository<>));

builder.Services.AddScoped<ApexWorld_Backend.Common.Interfaces.IUnitOfWork, ApexWorld_Backend.Data.UnitOfWork>();
builder.Services.AddSingleton<ApexWorld_Backend.Common.Services.IBulkheadService, ApexWorld_Backend.Common.Services.BulkheadService>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Booking.Services.IBookingService, ApexWorld_Backend.Features.Booking.Services.BookingService>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Backups.Services.IBackupService, ApexWorld_Backend.Features.Backups.Services.BackupService>();

// Register Background Schedulers
builder.Services.AddHostedService<ApexWorld_Backend.Features.Backups.Scheduler.BackupScheduler>();
builder.Services.AddHostedService<ApexWorld_Backend.Features.Audit.Scheduler.AuditLogCleanupScheduler>();

builder.Services.AddScoped<ApexWorld_Backend.Features.Enquiry.Services.IEnquiryService, ApexWorld_Backend.Features.Enquiry.Services.EnquiryService>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Loan.Services.ILoanService, ApexWorld_Backend.Features.Loan.Services.LoanService>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Payment.Services.IPaymentService, ApexWorld_Backend.Features.Payment.Services.PaymentService>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Property.Services.IPropertyQueryService, ApexWorld_Backend.Features.Property.Services.PropertyService>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Property.Services.IPropertyCommandService, ApexWorld_Backend.Features.Property.Services.PropertyService>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Property.Services.IPropertyCancellationSagaService, ApexWorld_Backend.Features.Property.Services.PropertyCancellationSagaService>();

// Register Property Rules & Validators
builder.Services.AddScoped<ApexWorld_Backend.Features.Property.Rules.IPropertyRule, ApexWorld_Backend.Features.Property.Rules.ValidCategoryRule>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Property.Rules.IPropertyRule, ApexWorld_Backend.Features.Property.Rules.ValidPriceRule>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Property.Validators.PropertyRequestValidator>();

builder.Services.AddScoped<ApexWorld_Backend.Features.Users.Services.IAuthService, ApexWorld_Backend.Features.Users.Services.AuthService>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Users.Services.IUserService, ApexWorld_Backend.Features.Users.Services.UserService>();

builder.Services.AddScoped<ApexWorld_Backend.Features.Users.Rules.IUserProfileRule<ApexWorld_Backend.Features.Users.DTOs.UpdateBuyerProfileDto>, ApexWorld_Backend.Features.Users.Rules.BuyerEmailFormatRule>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Users.Rules.IUserProfileRule<ApexWorld_Backend.Features.Users.DTOs.UpdateBuyerProfileDto>, ApexWorld_Backend.Features.Users.Rules.BuyerPhoneNumberFormatRule>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Users.Rules.IUserProfileRule<ApexWorld_Backend.Features.Users.DTOs.UpdateAdminProfileDto>, ApexWorld_Backend.Features.Users.Rules.AdminRoleRule>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Users.Validators.UpdateBuyerProfileValidator>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Users.Validators.UpdateAdminProfileValidator>();

// Register Report Rules & Services
builder.Services.AddScoped<ApexWorld_Backend.Features.Reports.Rules.IReportRule, ApexWorld_Backend.Features.Reports.Rules.ValidReportTypeRule>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Reports.Rules.IReportRule, ApexWorld_Backend.Features.Reports.Rules.ValidReportFormatRule>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Reports.Rules.IReportRule, ApexWorld_Backend.Features.Reports.Rules.ValidReportDateRule>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Reports.Rules.IReportRule, ApexWorld_Backend.Features.Reports.Rules.ValidSortByRule>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Reports.Validators.ReportRequestValidator>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Reports.Services.IDocumentGeneratorService, ApexWorld_Backend.Features.Reports.Services.DocumentGeneratorService>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Reports.Services.IReportService, ApexWorld_Backend.Features.Reports.Services.ReportService>();

builder.Services.AddScoped<ApexWorld_Backend.Features.Wishlist.Services.IWishlistService, ApexWorld_Backend.Features.Wishlist.Services.WishlistService>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Wishlist.Rules.IWishlistRule<int>, ApexWorld_Backend.Features.Wishlist.Rules.ValidPropertyIdRule>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Wishlist.Validators.WishlistRequestValidator>();

// Register MemoryCache for Idempotency Filter
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ApexWorld_Backend.Filters.IdempotencyFilter>();

// Register Review Rules & Services
builder.Services.AddScoped<ApexWorld_Backend.Features.Review.Rules.IReviewRule<ApexWorld_Backend.Features.Review.DTOs.CreatePlatformReviewDto>, ApexWorld_Backend.Features.Review.Rules.PlatformRatingRule>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Review.Rules.IReviewRule<ApexWorld_Backend.Features.Review.DTOs.CreatePropertyReviewDto>, ApexWorld_Backend.Features.Review.Rules.PropertyRatingRule>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Review.Rules.IReviewRule<ApexWorld_Backend.Features.Review.DTOs.CreatePropertyReviewDto>, ApexWorld_Backend.Features.Review.Rules.MaxPhotosRule>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Review.Validators.PlatformReviewValidator>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Review.Validators.PropertyReviewValidator>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Review.Services.IReviewService, ApexWorld_Backend.Features.Review.Services.ReviewService>();

builder.Services.AddScoped<ApexWorld_Backend.Features.Notifications.Services.IBuyerNotificationService, ApexWorld_Backend.Features.Notifications.Services.BuyerNotificationService>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Notifications.Services.IAdminNotificationService, ApexWorld_Backend.Features.Notifications.Services.AdminNotificationService>();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ApexWorld_Backend.Common.Interfaces.ICurrentUserService, ApexWorld_Backend.Common.Services.CurrentUserService>();
builder.Services.AddScoped<ApexWorld_Backend.Common.Interfaces.IAuditService, ApexWorld_Backend.Features.Audit.Services.AuditService>();
builder.Services.AddScoped<ApexWorld_Backend.Features.Audit.Services.IAuditQueryService, ApexWorld_Backend.Features.Audit.Services.AuditService>();

// AI Companion Setup
builder.Services.AddTransient<PropertyAgentPlugin>();
builder.Services.AddTransient<Kernel>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("Gemini__ApiKey") ?? "";
    var modelId = config["Gemini:ModelId"] ?? Environment.GetEnvironmentVariable("Gemini__ModelId") ?? "gemini-1.5-flash";

    var kernelBuilder = Kernel.CreateBuilder();
    var httpClient = new System.Net.Http.HttpClient(new GeminiRoleFixingHandler { InnerHandler = new System.Net.Http.HttpClientHandler() });
#pragma warning disable SKEXP0070
    kernelBuilder.AddGoogleAIGeminiChatCompletion(modelId: modelId, apiKey: apiKey, httpClient: httpClient);
#pragma warning restore SKEXP0070
    kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<PropertyAgentPlugin>());
    return kernelBuilder.Build();
});

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience, RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? "super-secret-key-change-this-in-production"))
    };
});

// Output Caching
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("PropertyCache", builder => builder.Expire(TimeSpan.FromMinutes(10)).Tag("properties"));
});

// Hangfire Background Processing (In-Memory)
builder.Services.AddHangfire(configuration => configuration
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage());
builder.Services.AddHangfireServer();

// Polly Resilience (Circuit Breaker & Retry for External Services e.g. Payment Gateway)
builder.Services.AddHttpClient("PaymentGateway")
    .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
    .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.CircuitBreakerAsync(5, TimeSpan.FromMinutes(1)));

// Webhook Dispatch HttpClient with Polly Retries
builder.Services.AddHttpClient("WebhookDispatcher")
    .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

// Register Global ExponentialBackoffRetryPolicy
builder.Services.AddSingleton<ApexWorld_Backend.Common.Resilience.ExponentialBackoffRetryPolicy>();

// Register Webhook Dispatch Service
builder.Services.AddScoped<ApexWorld_Backend.Features.Webhooks.Services.IWebhookDispatchService, ApexWorld_Backend.Features.Webhooks.Services.WebhookDispatchService>();

// Register DLQ Service
builder.Services.AddScoped<ApexWorld_Backend.Features.BackgroundJobs.Services.IDeadLetterQueueService, ApexWorld_Backend.Features.BackgroundJobs.Services.DeadLetterQueueService>();

// Register Email Service
builder.Services.AddScoped<ApexWorld_Backend.Core.Interfaces.IEmailService, ApexWorld_Backend.Core.Services.SmtpEmailService>();

builder.Services.AddSignalR();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<ApexWorld_Backend.Swagger.IdempotencyKeyHeaderFilter>();
    options.OperationFilter<ApexWorld_Backend.Swagger.SwaggerDocumentTagFilter>();
    options.SwaggerDoc("admin", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Admin API", Version = "v1" });
    options.SwaggerDoc("buyer", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Buyer API", Version = "v1" });
    options.SwaggerDoc("subadmin", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "SubAdmin API", Version = "v1" });
    options.SwaggerDoc("public", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Public & Shared API", Version = "v1" });

    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out var methodInfo)) return false;
        
        var classTagsAttr = methodInfo.DeclaringType?.GetCustomAttributes(typeof(Microsoft.AspNetCore.Http.TagsAttribute), true).FirstOrDefault() as Microsoft.AspNetCore.Http.TagsAttribute;
        var methodTagsAttr = methodInfo.GetCustomAttributes(typeof(Microsoft.AspNetCore.Http.TagsAttribute), true).FirstOrDefault() as Microsoft.AspNetCore.Http.TagsAttribute;
        
        var tags = new List<string>();
        if (classTagsAttr?.Tags != null) tags.AddRange(classTagsAttr.Tags);
        if (methodTagsAttr?.Tags != null) tags.AddRange(methodTagsAttr.Tags);
        
        if (!tags.Any()) return docName == "public";

        var tagStrings = tags.Select(t => t.ToLower()).ToList();

        if (docName == "admin" && tagStrings.Any(t => t.StartsWith("admin"))) return true;
        if (docName == "buyer" && tagStrings.Any(t => t.StartsWith("buyer"))) return true;
        if (docName == "subadmin" && tagStrings.Any(t => t.StartsWith("subadmin"))) return true;
        if (docName == "public" && tagStrings.Any(t => t.StartsWith("public") || t.StartsWith("shared"))) return true;

        return false;
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Just enter your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Fixed", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 5;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
    options.RejectionStatusCode = 429;
});

var app = builder.Build();

// Add Global Exception Middleware first so it catches all errors in the pipeline
app.UseMiddleware<ApexWorld_Backend.Middleware.GlobalExceptionMiddleware>();

// Add Correlation ID Middleware
app.UseMiddleware<ApexWorld_Backend.Middleware.CorrelationIdMiddleware>();

// Add Request Logging Middleware
app.UseMiddleware<ApexWorld_Backend.Middleware.RequestLoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/admin/swagger.json", "Admin API");
        c.SwaggerEndpoint("/swagger/buyer/swagger.json", "Buyer API");
        c.SwaggerEndpoint("/swagger/subadmin/swagger.json", "SubAdmin API");
        c.SwaggerEndpoint("/swagger/public/swagger.json", "Public & Shared API");
        c.DocumentTitle = "Apex World REPMS - Swagger UI";
        c.EnablePersistAuthorization();
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseRateLimiter();

app.UseOutputCache();   

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ApexWorld_Backend.Hubs.NotificationHub>("/hubs/notifications");

// SPA Fallback: serve index.html for all non-API routes (enables Angular client-side routing on refresh)
app.MapFallbackToFile("index.html");

// Seed Superadmin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        var adminRole = context.Set<ApexWorld_Backend.Features.Roles.Models.Role>().FirstOrDefault(r => r.RoleName == ApexWorld.Core.Common.Roles.Admin);
        if (adminRole == null)
        {
            adminRole = new ApexWorld_Backend.Features.Roles.Models.Role { RoleName = ApexWorld.Core.Common.Roles.Admin };
            context.Set<ApexWorld_Backend.Features.Roles.Models.Role>().Add(adminRole);
            context.SaveChanges();
        }

        var buyerRole = context.Set<ApexWorld_Backend.Features.Roles.Models.Role>().FirstOrDefault(r => r.RoleName == ApexWorld.Core.Common.Roles.Buyer);
        if (buyerRole == null)
        {
            buyerRole = new ApexWorld_Backend.Features.Roles.Models.Role { RoleName = ApexWorld.Core.Common.Roles.Buyer };
            context.Set<ApexWorld_Backend.Features.Roles.Models.Role>().Add(buyerRole);
            context.SaveChanges();
        }

        if (!context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).Any(u => u.UserRoles.Any(ur => ur.Role!.RoleName == ApexWorld.Core.Common.Roles.Admin)))
        {
            var superAdmin = new ApexWorld_Backend.Features.Users.Models.Admin
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Department = "Global",
                UserRoles = new List<ApexWorld_Backend.Features.Roles.Models.UserRole>
                {
                    new ApexWorld_Backend.Features.Roles.Models.UserRole { Role = adminRole }
                }
            };
            context.Users.Add(superAdmin);
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        // Log error
        Console.WriteLine($"An error occurred seeding the DB: {ex.Message}");
    }
}

// Schedule background jobs
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<ApexWorld_Backend.Features.Booking.Services.IBookingService>(
        "cancel-stale-bookings",
        service => service.CancelStaleBookingsAsync(),
        "*/10 * * * *");
        
    recurringJobManager.AddOrUpdate<ApexWorld_Backend.Features.BackgroundJobs.Services.IDeadLetterQueueService>(
        "process-dlq",
        service => service.ProcessDeadLetterQueueAsync(),
        "*/15 * * * *");
}

app.Run();

public class GeminiRoleFixingHandler : System.Net.Http.DelegatingHandler
{
    protected override async Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        if (request.Content != null)
        {
            var oldContent = request.Content;
            var reqContent = await oldContent.ReadAsStringAsync(cancellationToken);
            
            // Log the original request
            System.Console.WriteLine($"[Gemini Request] URL: {request.RequestUri}");
            System.Console.WriteLine($"[Gemini Request (Original)] Body: {reqContent}");

            // Fix the role "function" bug in the Semantic Kernel Google Connector
            if (reqContent.Contains("\"role\":\"function\""))
            {
                reqContent = reqContent.Replace("\"role\":\"function\"", "\"role\":\"user\"");
                System.Console.WriteLine($"[Gemini Request (Fixed)] Body: {reqContent}");
            }

            request.Content = new System.Net.Http.StringContent(reqContent, System.Text.Encoding.UTF8, "application/json");
            foreach (var header in oldContent.Headers)
            {
                if (header.Key != "Content-Type" && header.Key != "Content-Length")
                {
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }
        
        var response = await base.SendAsync(request, cancellationToken);
        
        if (response.Content != null)
        {
            var resContent = await response.Content.ReadAsStringAsync(cancellationToken);
            System.Console.WriteLine($"[Gemini Response] Status: {response.StatusCode}");
            System.Console.WriteLine($"[Gemini Response] Body: {resContent}");
        }
        return response;
    }
}
