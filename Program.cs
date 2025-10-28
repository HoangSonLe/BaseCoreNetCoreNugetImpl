using BaseNetCore.Core.src.Main.DAL.Repository;
using BaseNetCore.Core.src.Main.Database.PostgresSQL;
using BaseNetCore.Core.src.Main.Extensions;
using BaseSourceImpl.Application.Mappings;
using BaseSourceImpl.Application.Services.Implementations.User;
using BaseSourceImpl.Application.Services.Interfaces;
using BaseSourceImpl.Domains;
using BaseSourceImpl.Presentation.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NLog.Web;
using System.Reflection;
using System.Text.Json.Serialization;

var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

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
      options.UseNpgsql(connectionString));

    // Register PostgresDBContext as ApplicationDbContext for backward compatibility
    builder.Services.AddScoped<PostgresDBContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
    #endregion

    #region AUTOMAPPER
    builder.Services.AddAutoMapper(cfg =>
    {
        // Register AutoMapper Profiles
        cfg.AddProfile<UserProfile>();      // Application Layer
        cfg.AddProfile<UserRequestProfile>();    // Presentation Layer
    });
    #endregion

    #region CONTROLLERS
    builder.Services.AddControllers()
        .AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.AddControllersWithViews();
    #endregion

    builder.Services.AddBaseNetCoreFeaturesWithAuth(builder.Configuration);

    #region SWAGGER
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        // OpenAPI Info with explicit version
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "User Management API",
            Version = "v1",
            Description = "Clean Architecture API with .NET 8 and AutoMapper"
        });

        // JWT Bearer Authentication
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
        });

        // Add security requirement
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

        // Configure schema generation
        options.UseInlineDefinitionsForEnums();
        options.SupportNonNullableReferenceTypes();
    });
    #endregion

    #region NLOG
    logger.Debug("Application starting...");
    builder.Logging.ClearProviders();
    builder.Logging.AddDebug();
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Trace);
    builder.Host.UseNLog();
    #endregion

    #region DEPENDENCY INJECTION

    // Infrastructure Layer - UnitOfWork from Core Package
    builder.Services.AddScoped<IUnitOfWork>(sp =>
    {
        var dbContext = sp.GetRequiredService<PostgresDBContext>();
        return new UnitOfWork(dbContext);
    });

    // Application Layer - Services (AutoMapper injected automatically)
    builder.Services.AddScoped<IUserService, UserService>();

    #endregion

    #region APP CONFIGURATION
    var app = builder.Build();
    var environment = app.Environment;

    // Ensure database is created
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
        logger.Info("Database ensured created");
    }

    // Development settings
    if (environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    // Production settings
    if (!environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    // Middleware pipeline - CORRECT ORDER
    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    // Swagger configuration
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
        options.RoutePrefix = "swagger"; // Swagger UI at /swagger
    });

    app.UseCors(x => x
        .SetIsOriginAllowed(origin => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
   .AllowCredentials());

    app.UseBaseNetCoreMiddlewareWithAuth();

    // Map controllers
    app.MapControllers();

    // Redirect root to Swagger UI
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    logger.Info("Application started successfully");
    app.Run();
    #endregion
}
catch (Exception ex)
{
    logger.Error(ex, "Error in init");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}
