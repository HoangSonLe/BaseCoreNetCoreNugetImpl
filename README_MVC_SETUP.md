# ?? Setup MVC Project with BaseNetCore.Core Library

## ?? Table of Contents
- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Project Setup](#project-setup)
- [Authentication Configuration](#authentication-configuration)
- [Project Structure](#project-structure)
- [Step-by-Step Implementation](#step-by-step-implementation)
- [Differences: API vs MVC](#differences-api-vs-mvc)
- [Common Issues](#common-issues)

---

## ?? Overview

This guide explains how to setup an **ASP.NET Core MVC project** using the **BaseNetCore.Core** library with Cookie-based authentication, compared to the API project which uses JWT Bearer tokens.

**Key Differences:**
- **API Project**: JWT Bearer Token (Stateless)
- **MVC Project**: Cookie Authentication (Server-side session)

---

## ? Prerequisites

- **.NET 8 SDK**
- **Visual Studio 2022** or **VS Code**
- **PostgreSQL** (or another database)
- **BaseNetCore.Core** library (v1.0.5-beta or higher)

---

## ?? Project Setup

### 1. Create MVC Project

```bash
# Create new MVC project
dotnet new mvc -n MyMvcApp
cd MyMvcApp

# Add BaseNetCore.Core package
dotnet add package BaseNetCore.CoreLibrary --version 1.0.5-beta

# Add required packages
dotnet add package Microsoft.AspNetCore.Authentication.Cookies
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add package NLog.Web.AspNetCore
```

### 2. Project Structure

```
MyMvcApp/
??? Controllers/
?   ??? HomeController.cs        # Public pages
?   ??? AccountController.cs     # Login/Logout
?   ??? UsersController.cs       # Protected CRUD
??? Views/
?   ??? Shared/
?   ?   ??? _Layout.cshtml
?   ? ??? _LoginPartial.cshtml
?   ??? Home/
? ?   ??? Index.cshtml
?   ??? Account/
?   ?   ??? Login.cshtml
?   ?   ??? AccessDenied.cshtml
?   ??? Users/
?       ??? Index.cshtml
? ??? Create.cshtml
?  ??? Edit.cshtml
??? Models/
?   ??? ViewModels/
?       ??? LoginViewModel.cs
???? UserViewModel.cs
??? wwwroot/         # Static files (CSS, JS, images)
? ??? css/
?   ??? js/
?   ??? lib/
??? Application/      # Business Logic Layer
??? Domains/     # Domain Entities
??? Program.cs
??? appsettings.json
```

---

## ?? Authentication Configuration

### Step 1: Update `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost:5433;Database=MyMvcDb;User Id=postgres;Password=123456"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*",
  
  "CookieSettings": {
    "CookieName": "MyMvcApp.Auth",
    "LoginPath": "/Account/Login",
 "LogoutPath": "/Account/Logout",
    "AccessDeniedPath": "/Account/AccessDenied",
    "ExpirationMinutes": 60,
    "SlidingExpiration": true
  },
  
  "SessionSettings": {
    "IdleTimeout": 20,
    "CookieName": "MyMvcApp.Session",
    "IsEssential": true
  }
}
```

### Step 2: Create Cookie Authentication Extension

Create file: `Extensions/MvcAuthenticationExtensions.cs`

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MyMvcApp.Extensions
{
    public static class MvcAuthenticationExtensions
  {
        public static IServiceCollection AddMvcCookieAuthentication(
  this IServiceCollection services,
            IConfiguration configuration)
     {
            // Get cookie settings from configuration
          var cookieSection = configuration.GetSection("CookieSettings");
       var loginPath = cookieSection.GetValue<string>("LoginPath") ?? "/Account/Login";
            var logoutPath = cookieSection.GetValue<string>("LogoutPath") ?? "/Account/Logout";
  var accessDeniedPath = cookieSection.GetValue<string>("AccessDeniedPath") ?? "/Account/AccessDenied";
      var expirationMinutes = cookieSection.GetValue<int>("ExpirationMinutes", 60);
   var slidingExpiration = cookieSection.GetValue<bool>("SlidingExpiration", true);

            // Configure Cookie Authentication
    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
     .AddCookie(options =>
            {
            options.LoginPath = loginPath;
        options.LogoutPath = logoutPath;
              options.AccessDeniedPath = accessDeniedPath;
             options.ExpireTimeSpan = TimeSpan.FromMinutes(expirationMinutes);
   options.SlidingExpiration = slidingExpiration;
          
            options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
 options.Cookie.SameSite = SameSiteMode.Lax;
         options.Cookie.Name = cookieSection.GetValue<string>("CookieName") ?? "MyMvcApp.Auth";

     // Handle cookie events
    options.Events = new CookieAuthenticationEvents
         {
       OnRedirectToLogin = context =>
         {
       // For AJAX requests, return 401 instead of redirect
     if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
    {
                context.Response.StatusCode = 401;
              return Task.CompletedTask;
       }
   context.Response.Redirect(context.RedirectUri);
 return Task.CompletedTask;
  },
            OnRedirectToAccessDenied = context =>
          {
                if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
          {
      context.Response.StatusCode = 403;
     return Task.CompletedTask;
        }
    context.Response.Redirect(context.RedirectUri);
         return Task.CompletedTask;
 }
    };
    });

      return services;
        }
    }
}
```

### Step 3: Configure `Program.cs`

```csharp
using BaseNetCore.Core.src.Main.DAL.Repository;
using BaseNetCore.Core.src.Main.Database.PostgresSQL;
using BaseNetCore.Core.src.Main.Extensions;
using MyMvcApp.Application.Services.User;
using MyMvcApp.Application.Services.Auth;
using MyMvcApp.Domains;
using MyMvcApp.Extensions;
using Microsoft.EntityFrameworkCore;
using NLog.Web;

var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    #region Configuration
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
      .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();
    #endregion

    #region Database
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
     options.UseNpgsql(connectionString));
    
    builder.Services.AddScoped<PostgresDBContext>(sp => 
        sp.GetRequiredService<ApplicationDbContext>());
    #endregion

    #region AutoMapper
    builder.Services.AddAutoMapper(typeof(Program).Assembly);
    #endregion

    #region MVC & Session
    builder.Services.AddControllersWithViews();

    // Add Session support
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(
       builder.Configuration.GetValue<int>("SessionSettings:IdleTimeout", 20));
 options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = builder.Configuration.GetValue<string>("SessionSettings:CookieName") 
   ?? "MyMvcApp.Session";
    });
    
    builder.Services.AddHttpContextAccessor();
    #endregion

    #region Authentication - MVC Cookie-based
    builder.Services.AddMvcCookieAuthentication(builder.Configuration);
    #endregion

    #region Core Features (WITHOUT JWT Auth)
    // Use BaseNetCore.Core features but WITHOUT JWT authentication
    builder.Services.AddBaseNetCoreFeatures(builder.Configuration);
  builder.Services.AddDistributedMemoryCache();
    #endregion

    #region Logging - NLog
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(LogLevel.Trace);
    builder.Host.UseNLog();
    #endregion

    #region Dependency Injection
    // UnitOfWork
    builder.Services.AddScoped<IUnitOfWork>(sp =>
    {
  var dbContext = sp.GetRequiredService<PostgresDBContext>();
        return new UnitOfWork(dbContext);
    });

    // Services
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
 #endregion

    #region App Configuration
    var app = builder.Build();

    // Ensure database created
    using (var scope = app.Services.CreateScope())
    {
   var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
    }

    // Error handling
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
    else
    {
   app.UseDeveloperExceptionPage();
    }

    // Middleware pipeline - CORRECT ORDER
    app.UseHttpsRedirection();
    app.UseStaticFiles();   // ?? IMPORTANT for CSS/JS
    
    app.UseRouting();
    
    app.UseSession();      // ?? Before Authentication
    app.UseAuthentication();       // ?? Cookie Authentication
    app.UseAuthorization();

    // Use BaseNetCore middleware (exception handling, validation)
    app.UseBaseNetCoreMiddleware();

    // MVC Routes
    app.MapControllerRoute(
    name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    logger.Info("MVC Application started successfully");
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
```

---

## ?? Step-by-Step Implementation

### 1. Create AccountController

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Application.Services.Auth;
using MyMvcApp.Models.ViewModels;
using System.Security.Claims;

namespace MyMvcApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
      private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
  _authService = authService;
    _logger = logger;
        }

        [HttpGet]
   [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
  ViewData["ReturnUrl"] = returnUrl;
      return View();
  }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

         if (!ModelState.IsValid)
        {
   return View(model);
  }

            try
            {
                var user = await _authService.ValidateUserAsync(model.UserName, model.Password);

        if (user == null)
       {
   ModelState.AddModelError(string.Empty, "Invalid username or password.");
          return View(model);
      }

            // Create claims
                var claims = new List<Claim>
        {
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
   new Claim(ClaimTypes.Name, user.UserName),
             new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
new Claim("TypeAccount", user.TypeAccount.ToString())
       };

       // Add roles
       foreach (var roleId in user.RoleIdList)
           {
   claims.Add(new Claim(ClaimTypes.Role, roleId.ToString()));
             }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
   var authProperties = new AuthenticationProperties
       {
     IsPersistent = model.RememberMe,
              ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.RememberMe ? 24 : 1)
       };

  await HttpContext.SignInAsync(
     CookieAuthenticationDefaults.AuthenticationScheme,
    new ClaimsPrincipal(claimsIdentity),
      authProperties);

       _logger.LogInformation($"User {model.UserName} logged in successfully.");

          if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
    {
      return Redirect(returnUrl);
}

        return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
   {
            _logger.LogError(ex, "Error during login");
   ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
              return View(model);
      }
    }

        [HttpPost]
     [Authorize]
     [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
     {
     await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
_logger.LogInformation("User logged out.");
  return RedirectToAction("Index", "Home");
        }

        [HttpGet]
      [AllowAnonymous]
        public IActionResult AccessDenied()
      {
  return View();
    }
    }
}
```

### 2. Create Login View

Create file: `Views/Account/Login.cshtml`

```cshtml
@model MyMvcApp.Models.ViewModels.LoginViewModel
@{
    ViewData["Title"] = "Login";
}

<div class="row justify-content-center">
    <div class="col-md-4">
        <h2>@ViewData["Title"]</h2>
        <hr />
  <form asp-action="Login" method="post">
          <div asp-validation-summary="ModelOnly" class="text-danger"></div>
    
            <div class="form-group">
           <label asp-for="UserName"></label>
         <input asp-for="UserName" class="form-control" />
      <span asp-validation-for="UserName" class="text-danger"></span>
          </div>
  
  <div class="form-group">
          <label asp-for="Password"></label>
          <input asp-for="Password" class="form-control" type="password" />
         <span asp-validation-for="Password" class="text-danger"></span>
            </div>
   
            <div class="form-check">
 <input asp-for="RememberMe" class="form-check-input" />
         <label asp-for="RememberMe" class="form-check-label">Remember me</label>
            </div>
    
      <button type="submit" class="btn btn-primary mt-3">Login</button>
        </form>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

### 3. Create LoginViewModel

```csharp
using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
```

### 4. Update _Layout.cshtml

Add login/logout partial view:

```cshtml
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - MyMvcApp</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <header>
        <nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3">
 <div class="container-fluid">
      <a class="navbar-brand" asp-area="" asp-controller="Home" asp-action="Index">MyMvcApp</a>
  <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target=".navbar-collapse">
     <span class="navbar-toggler-icon"></span>
 </button>
          <div class="navbar-collapse collapse d-sm-inline-flex justify-content-between">
      <ul class="navbar-nav flex-grow-1">
         <li class="nav-item">
       <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="Index">Home</a>
           </li>
        @if (User.Identity.IsAuthenticated)
      {
            <li class="nav-item">
             <a class="nav-link text-dark" asp-area="" asp-controller="Users" asp-action="Index">Users</a>
             </li>
     }
        </ul>
      <partial name="_LoginPartial" />
    </div>
</div>
     </nav>
    </header>
    <div class="container">
        <main role="main" class="pb-3">
          @RenderBody()
        </main>
    </div>

    <footer class="border-top footer text-muted">
     <div class="container">
   &copy; 2024 - MyMvcApp
     </div>
    </footer>

    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
  <script src="~/js/site.js" asp-append-version="true"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

### 5. Create _LoginPartial.cshtml

```cshtml
@if (User.Identity.IsAuthenticated)
{
    <ul class="navbar-nav">
      <li class="nav-item">
       <span class="navbar-text">Hello, @User.Identity.Name!</span>
        </li>
     <li class="nav-item">
            <form asp-controller="Account" asp-action="Logout" method="post" class="form-inline">
         <button type="submit" class="btn btn-link nav-link">Logout</button>
            </form>
  </li>
    </ul>
}
else
{
    <ul class="navbar-nav">
        <li class="nav-item">
         <a class="nav-link text-dark" asp-controller="Account" asp-action="Login">Login</a>
        </li>
    </ul>
}
```

### 6. Create Protected Controller

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Application.Services.User;

namespace MyMvcApp.Controllers
{
    [Authorize] // ?? Require authentication
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

   public UsersController(IUserService userService)
      {
      _userService = userService;
}

        [HttpGet]
   public async Task<IActionResult> Index()
        {
var users = await _userService.GetAllAsync();
          return View(users.Value);
        }

      [HttpGet]
        [Authorize(Roles = "Admin")] // ?? Role-based authorization
   public IActionResult Create()
    {
            return View();
        }

 [HttpPost]
        [ValidateAntiForgeryToken] // ?? CSRF protection
        [Authorize(Roles = "Admin")]
      public async Task<IActionResult> Create(UserViewModel model)
        {
     if (!ModelState.IsValid)
      {
       return View(model);
  }

    await _userService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
   }
    }
}
```

---

## ?? Differences: API vs MVC

| Feature | API Project | MVC Project |
|---------|-------------|-------------|
| **Authentication** | JWT Bearer Token | Cookie Authentication |
| **Response Type** | JSON (`ApiResponse`) | HTML Views (`.cshtml`) |
| **Authorization** | `Authorization: Bearer {token}` | Cookie sent automatically |
| **CSRF Protection** | ? Not needed | ? `[ValidateAntiForgeryToken]` |
| **CORS** | ? Required | ? Not needed |
| **Swagger** | ? Enabled | ? Not applicable |
| **Static Files** | ? Not used | ? `app.UseStaticFiles()` |
| **Session** | ? Stateless | ? Server-side session |
| **Controller Base** | `ControllerBase` | `Controller` |
| **Return Type** | `IActionResult` with JSON | `IActionResult` with View |
| **Error Handling** | JSON error response | Redirect to Error view |
| **Mobile-friendly** | ? Perfect | ? Not suitable |

### API Project Configuration (Current)
```csharp
// Program.cs
builder.Services.AddControllers();
builder.Services.AddJwtAuthentication(builder.Configuration); // JWT Token
app.UseBaseNetCoreMiddlewareWithAuth(); // API middleware
```

### MVC Project Configuration (New)
```csharp
// Program.cs
builder.Services.AddControllersWithViews();
builder.Services.AddMvcCookieAuthentication(builder.Configuration); // Cookie Auth
builder.Services.AddSession(); // Session support
app.UseStaticFiles(); // CSS/JS files
app.UseBaseNetCoreMiddleware(); // MVC middleware (without JWT)
```

---

## ?? Common Issues

### 1. **401 Unauthorized Loop**
**Problem:** Redirects to login page infinitely

**Solution:**
```csharp
// Check LoginPath in appsettings.json
"CookieSettings": {
  "LoginPath": "/Account/Login",  // ? Must match controller route
}

// Make sure Login action allows anonymous
[AllowAnonymous]
public IActionResult Login() { }
```

### 2. **CSRF Token Validation Failed**
**Problem:** POST requests fail with "The antiforgery token could not be validated"

**Solution:**
```cshtml
<!-- Always include in forms -->
<form asp-action="Create" method="post">
    @Html.AntiForgeryToken()  <!-- ? Add this -->
    <!-- form fields -->
</form>
```

```csharp
// Controller action
[HttpPost]
[ValidateAntiForgeryToken]  // ? Add this attribute
public async Task<IActionResult> Create(UserViewModel model) { }
```

### 3. **Static Files Not Loading**
**Problem:** CSS/JS files return 404

**Solution:**
```csharp
// Program.cs - MUST be before UseRouting
app.UseStaticFiles();  // ? Add this
app.UseRouting();
```

### 4. **Session Not Working**
**Problem:** Session data is lost between requests

**Solution:**
```csharp
// Program.cs - Order matters!
app.UseSession();        // ? BEFORE UseAuthentication
app.UseAuthentication();
app.UseAuthorization();
```

### 5. **Cannot Use BaseNetCore.Core JWT Features**
**Problem:** Want to use both Cookie and JWT

**Solution:**
```csharp
// You can use both authentication schemes
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(/* cookie options */)
    .AddJwtBearer(/* jwt options */);

// In controller, specify scheme
[Authorize(AuthenticationSchemes = "Bearer")]  // JWT only
[Authorize(AuthenticationSchemes = "Cookies")] // Cookie only
[Authorize] // Default (Cookie)
```

---

## ?? Additional Resources

### BaseNetCore.Core Extension Methods

**For MVC (without JWT):**
```csharp
builder.Services.AddBaseNetCoreFeatures(configuration);
app.UseBaseNetCoreMiddleware();
```

**For API (with JWT):**
```csharp
builder.Services.AddBaseNetCoreFeaturesWithAuth(configuration);
app.UseBaseNetCoreMiddlewareWithAuth();
```

### NuGet Package
```bash
dotnet add package BaseNetCore.CoreLibrary --version 1.0.5-beta
```

### Repository Links
- **Core Library**: https://github.com/HoangSonLe/BaseCoreNetCoreNuget
- **Implementation Example**: https://github.com/HoangSonLe/BaseCoreNetCoreNugetImpl

---

## ? Checklist

- [ ] Install required NuGet packages
- [ ] Update `appsettings.json` with Cookie settings
- [ ] Create `MvcAuthenticationExtensions.cs`
- [ ] Update `Program.cs` with MVC configuration
- [ ] Create `AccountController` with Login/Logout
- [ ] Create Login view with CSRF token
- [ ] Update `_Layout.cshtml` with navigation
- [ ] Create `_LoginPartial.cshtml`
- [ ] Test login/logout functionality
- [ ] Verify protected routes require authentication
- [ ] Test CSRF protection on POST requests

---

## ?? Summary

### Key Takeaways:
1. **MVC uses Cookie Authentication** (not JWT)
2. **Always use `[ValidateAntiForgeryToken]`** on POST actions
3. **Order matters** in middleware pipeline
4. **Use `app.UseStaticFiles()`** for CSS/JS
5. **Session support** is optional but recommended
6. **BaseNetCore.Core** works with both API and MVC projects

### When to Use:
- **API Project**: Mobile apps, SPAs, microservices
- **MVC Project**: Traditional web apps, server-side rendering

---

**Created by:** BaseNetCore.Core Team  
**Version:** 1.0.0  
**Last Updated:** 2024-01-18
