using BaseNetCore.Core.src.Main.DAL.Repository;
using BaseNetCore.Core.src.Main.Database.PostgresSQL;
using BaseNetCore.Core.src.Main.Extensions;
using BaseNetCore.Core.src.Main.Extensions.Performance;
using BaseNetCore.Core.src.Main.Extensions.Permission;
using BaseNetCore.Core.src.Main.Extensions.RateLimited;
using BaseSourceImpl.Application.Mappings;
using BaseSourceImpl.Application.Services.Auth;
using BaseSourceImpl.Application.Services.Permission;
using BaseSourceImpl.Application.Services.TokenSession;
using BaseSourceImpl.Application.Services.User;
using BaseSourceImpl.Domains;
using BaseSourceImpl.Presentation.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// ✅ Configure Kestrel for high performance (OPTIONAL - uses defaults if config missing)
builder.WebHost.UseBaseNetCoreKestrelOptimization(builder.Configuration);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    var appAssembly = Assembly.Load(new AssemblyName(builder.Environment.ApplicationName));
    if (appAssembly != null)
        builder.Configuration.AddUserSecrets(appAssembly, optional: true);
}

#region DATABASE
var connectionString = builder.Configuration.GetConnectionString("SAMPLEDB");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(30);
        npgsqlOptions.MaxBatchSize(100);
    });
    //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddScoped<PostgresDBContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
#endregion

#region AUTOMAPPER
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<UserProfile>();
    cfg.AddProfile<UserRequestProfile>();
});
#endregion

#region CONTROLLERS
builder.Services.AddControllers()
    .AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddControllersWithViews();
#endregion

#region CORE SETTING
builder.Services.AddBaseNetCoreFeaturesWithAuth(configuration: builder.Configuration, builder: builder);
builder.Services.AddScoped<ITokenSessionService, TokenSessionService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddBaseCoreDynamicAuthorization();
builder.Services.AddBaseRateLimiting();

// ✅ Add Performance Optimization (OPTIONAL - uses defaults if config missing)
builder.Services.AddBaseNetCorePerformanceOptimization(builder.Configuration);
#endregion

#region SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "User Management API",
        Version = "v1",
        Description = "Clean Architecture API with .NET 8 and AutoMapper"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
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

    options.UseInlineDefinitionsForEnums();
    options.SupportNonNullableReferenceTypes();
});
#endregion

#region DEPENDENCY INJECTION
builder.Services.AddScoped<IUnitOfWork>(sp =>
{
    var dbContext = sp.GetRequiredService<PostgresDBContext>();
    return new UnitOfWork(dbContext);
});

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
#endregion

#region APP CONFIGURATION   
var app = builder.Build();
var environment = app.Environment;

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

if (environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

if (!environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware pipeline - OPTIMIZED ORDER
app.UseHttpsRedirection();

// ✅ Use Performance Optimization (OPTIONAL - uses defaults if config missing)
app.UseBaseNetCorePerformanceOptimization(builder.Configuration);

app.UseStaticFiles();
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors(x => x
    .SetIsOriginAllowed(origin => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseBaseNetCoreMiddlewareWithAuth();
app.UseBaseCoreDynamicPermissionMiddleware();
//app.UseBaseRateLimiting();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
#endregion
try
{
    app.Run();
}
finally
{
    app.FlushBaseNetCoreSerilog();
}
