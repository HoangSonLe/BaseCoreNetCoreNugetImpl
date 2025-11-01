# ?? H??ng D?n Chuy?n ??i BaseSourceImpl t? API sang MVC Project

## ?? M?c L?c
- [T?ng Quan](#t?ng-quan)
- [C?u Trúc Hi?n T?i (API)](#c?u-trúc-hi?n-t?i-api)
- [C?u Trúc Sau Chuy?n ??i (MVC)](#c?u-trúc-sau-chuy?n-??i-mvc)
- [Các B??c Chuy?n ??i](#các-b??c-chuy?n-??i)
- [Chi Ti?t Thay ??i](#chi-ti?t-thay-??i)
- [Testing & Verification](#testing--verification)
- [Rollback Plan](#rollback-plan)

---

## ?? T?ng Quan

Project **BaseSourceImpl** hi?n ?ang là **REST API** s? d?ng:
- ? JWT Bearer Authentication
- ? Swagger UI
- ? Clean Architecture (Application, Domain, Presentation layers)
- ? BaseNetCore.Core library
- ? PostgreSQL Database
- ? AutoMapper
- ? NLog

**M?c tiêu:** Chuy?n ??i sang **MVC Web Application** v?i Cookie Authentication, gi? nguyên business logic và database.

---

## ?? C?u Trúc Hi?n T?i (API)

```
BaseSourceImpl/
??? Application/
?   ??? DTOs/
?   ?   ??? User/
?   ?       ??? UserDto.cs
?   ?       ??? UserViewModel.cs
? ??? Mappings/
?   ?   ??? UserProfile.cs
?   ??? Services/
?   ?   ??? Auth/
?   ? ?   ??? IAuthService.cs
?   ?   ?   ??? AuthService.cs          # ?? C?n modify
?   ?   ??? User/
?   ?   ?   ??? IUserService.cs
?   ?   ?   ??? UserService.cs
?   ?   ??? TokenSession/
?   ?   ??? Permission/
?   ??? Validators/
??? Domains/
?   ??? Entities/
?   ?   ??? User/
?   ?   ??? Role/
?   ?   ??? Permission/
?   ?   ??? RefreshToken/# ?? Không c?n trong MVC
?   ?   ??? ...
?   ??? ApplicationDbContext.cs
??? Presentation/
?   ??? Controllers/
?       ??? Auth/
?       ?   ??? AuthController.cs      # ?? [ApiController] ? MVC Controller
?       ?   ??? Models/
?       ?   ??? LoginRequest.cs    # ?? ? LoginViewModel
?     ?       ??? JwtToken.cs        # ?? Không c?n
?       ??? User/
?           ??? UserController.cs      # ?? [ApiController] ? MVC Controller
?           ??? Models/
?            ??? CreateUserRequest.cs
?   ??? UpdateUserRequest.cs
??? Common/
?   ??? ErrorCodes/
?   ??? Enums/
??? Program.cs             # ?? C?n s?a ??i l?n
??? appsettings.json      # ?? Thêm Cookie settings
??? nlog.config
```

---

## ?? C?u Trúc Sau Chuy?n ??i (MVC)

```
BaseSourceImpl/
??? Application/# ? Gi? nguyên
??? Domains/          # ? Gi? nguyên
??? Presentation/
?   ??? Controllers/
?   ?   ??? HomeController.cs         # ?? Thêm m?i
?   ? ??? AccountController.cs       # ?? Thay th? AuthController
?   ?   ??? UsersController.cs         # ?? S?a ??i t? UserController
?   ??? Views/          # ?? Th? m?c m?i
?   ?   ??? Shared/
?   ?   ?   ??? _Layout.cshtml
?   ?   ?   ??? _LoginPartial.cshtml
?   ?   ?   ??? _ValidationScriptsPartial.cshtml
?   ? ?   ??? Error.cshtml
?   ?   ??? Home/
?   ?   ?   ??? Index.cshtml
? ?   ?   ??? Privacy.cshtml
?   ?   ??? Account/
?   ?   ?   ??? Login.cshtml
?   ?   ?   ??? AccessDenied.cshtml
??   ?   ??? Unauthorized.cshtml
?   ?   ??? Users/
?   ?       ??? Index.cshtml
?   ?       ??? Create.cshtml
?   ?     ??? Edit.cshtml
?   ?       ??? Details.cshtml
?   ?       ??? Delete.cshtml
?   ??? Models/   # ?? ViewModels cho Views
?       ??? ErrorViewModel.cs
?       ??? ViewModels/
?           ??? LoginViewModel.cs
? ??? UserViewModel.cs
?           ??? UserListViewModel.cs
??? wwwroot/   # ?? Static files
?   ??? css/
?   ?   ??? site.css
?   ?   ??? bootstrap.min.css
?   ??? js/
?   ?   ??? site.js
?   ?   ??? jquery.min.js
?   ??? lib/
?   ??? images/
??? Extensions/
? ??? MvcAuthenticationExtensions.cs # ?? Cookie Auth extension
??? Common/    # ? Gi? nguyên
??? Program.cs          # ?? S?a ??i l?n
??? appsettings.json     # ?? Thêm config
??? nlog.config    # ? Gi? nguyên
```

---

## ?? Các B??c Chuy?n ??i

### **B??c 1: Chu?n B? - Backup & Branch**

```bash
# T?o branch m?i cho MVC conversion
git checkout -b feature/convert-to-mvc

# Backup database (optional)
pg_dump -U postgres -d SampleNuget > backup_before_mvc.sql

# Commit current state
git add .
git commit -m "Backup before MVC conversion"
```

---

### **B??c 2: Cài ??t NuGet Packages**

```bash
# Remove JWT-specific packages (optional, có th? gi? n?u c?n hybrid)
# dotnet remove package Microsoft.AspNetCore.Authentication.JwtBearer

# Add Cookie Authentication (n?u ch?a có)
dotnet add package Microsoft.AspNetCore.Authentication.Cookies

# Verify packages
dotnet list package
```

**Expected packages:**
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.Cookies" Version="2.2.0" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.18" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.11" />
<PackageReference Include="NLog.Web.AspNetCore" Version="5.3.12" />
<!-- BaseNetCore.Core already referenced -->
```

---

### **B??c 3: T?o Cookie Authentication Extension**

**T?o file m?i:** `Extensions/MvcAuthenticationExtensions.cs`

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace BaseSourceImpl.Extensions
{
    /// <summary>
    /// Extension methods for MVC Cookie Authentication
/// </summary>
    public static class MvcAuthenticationExtensions
  {
        public static IServiceCollection AddMvcCookieAuthentication(
            this IServiceCollection services,
 IConfiguration configuration)
        {
            var cookieSection = configuration.GetSection("CookieSettings");
 
     services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
             .AddCookie(options =>
     {
          // Paths
       options.LoginPath = cookieSection.GetValue<string>("LoginPath") ?? "/Account/Login";
        options.LogoutPath = cookieSection.GetValue<string>("LogoutPath") ?? "/Account/Logout";
  options.AccessDeniedPath = cookieSection.GetValue<string>("AccessDeniedPath") ?? "/Account/AccessDenied";
     
          // Cookie settings
       options.Cookie.Name = cookieSection.GetValue<string>("CookieName") ?? "BaseSourceImpl.Auth";
    options.Cookie.HttpOnly = true;
             options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
      options.Cookie.SameSite = SameSiteMode.Lax;
         
    // Expiration
                  options.ExpireTimeSpan = TimeSpan.FromMinutes(
       cookieSection.GetValue<int>("ExpirationMinutes", 60));
   options.SlidingExpiration = cookieSection.GetValue<bool>("SlidingExpiration", true);
       
   // Events
             options.Events = new CookieAuthenticationEvents
         {
    OnRedirectToLogin = context =>
          {
          // For AJAX requests, return 401 instead of redirect
      if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
      context.Request.Headers["Accept"].ToString().Contains("application/json"))
       {
   context.Response.StatusCode = 401;
        return Task.CompletedTask;
       }
        
          context.Response.Redirect(context.RedirectUri);
    return Task.CompletedTask;
},
            OnRedirectToAccessDenied = context =>
         {
              if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
context.Request.Headers["Accept"].ToString().Contains("application/json"))
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

---

### **B??c 4: Update appsettings.json**

**Thêm vào `appsettings.json`:**

```json
{
  "ConnectionStrings": {
    "SAMPLEDB": "Server=localhost:5433;Database=SampleNuget;User Id=postgres;Password=123456"
  },
  "Logging": {
    "LogLevel": {
  "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  
  "CookieSettings": {
    "CookieName": "BaseSourceImpl.Auth",
    "LoginPath": "/Account/Login",
    "LogoutPath": "/Account/Logout",
    "AccessDeniedPath": "/Account/AccessDenied",
    "ExpirationMinutes": 60,
    "SlidingExpiration": true
  },
  
  "SessionSettings": {
    "IdleTimeout": 20,
    "CookieName": "BaseSourceImpl.Session",
    "IsEssential": true
  },
  
  "DynamicPermissions": {
    "PermitAll": [
  "/Account/Login",
 "/Account/Logout",
      "/Home/Index",
  "/Home/Privacy"
    ],
    "Permissions": {
      "base-service": [
    "/Users/Index:GET:@//USER_READ",
        "/Users/Details/*:GET:@//USER_READ",
        "/Users/Create:GET,POST:@//USER_WRITE",
     "/Users/Edit/*:GET,POST:@//USER_WRITE",
        "/Users/Delete/*:POST:@//USER_DELETE"
  ]
    }
  }
}
```

**?? Note:** 
- Remove ho?c comment out `TokenSettings` n?u không dùng JWT
- Gi? l?i n?u mu?n hybrid (cookie + API with JWT)

---

### **B??c 5: Update Program.cs**

**Thay th? toàn b? `Program.cs`:**

```csharp
using BaseNetCore.Core.src.Main.DAL.Repository;
using BaseNetCore.Core.src.Main.Database.PostgresSQL;
using BaseNetCore.Core.src.Main.Extensions;
using BaseSourceImpl.Application.Mappings;
using BaseSourceImpl.Application.Services.Auth;
using BaseSourceImpl.Application.Services.Permission;
using BaseSourceImpl.Application.Services.User;
using BaseSourceImpl.Domains;
using BaseSourceImpl.Extensions;
using BaseSourceImpl.Presentation.Mappings;
using Microsoft.EntityFrameworkCore;
using NLog.Web;
using System.Reflection;

var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    #region Configuration
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
#endregion

    #region Database
    var connectionString = builder.Configuration.GetConnectionString("SAMPLEDB");
  builder.Services.AddDbContext<ApplicationDbContext>(options =>
      options.UseNpgsql(connectionString));

    builder.Services.AddScoped<PostgresDBContext>(sp =>
        sp.GetRequiredService<ApplicationDbContext>());
    #endregion

    #region AutoMapper
    builder.Services.AddAutoMapper(cfg =>
    {
        cfg.AddProfile<UserProfile>();
        cfg.AddProfile<UserRequestProfile>();
    });
 #endregion

    #region MVC & Session
    // ? Change from AddControllers() to AddControllersWithViews()
    builder.Services.AddControllersWithViews();

    // ? Add Session support
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(
            builder.Configuration.GetValue<int>("SessionSettings:IdleTimeout", 20));
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
     options.Cookie.Name = builder.Configuration.GetValue<string>("SessionSettings:CookieName")
      ?? "BaseSourceImpl.Session";
    });

    builder.Services.AddHttpContextAccessor();
    #endregion

    #region Authentication - MVC Cookie
    // ? Replace JWT with Cookie Authentication
    builder.Services.AddMvcCookieAuthentication(builder.Configuration);
    #endregion

    #region Core Features
    // ? Use without JWT Auth (or keep AddBaseNetCoreFeaturesWithAuth if hybrid)
    builder.Services.AddBaseNetCoreFeatures(builder.Configuration);
    builder.Services.AddDistributedMemoryCache();
    
  // ?? Comment out if not using dynamic permissions in MVC
    // builder.Services.AddCoreDynamicAuthorization();
    #endregion

    #region NLog
    logger.Debug("MVC Application starting...");
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(LogLevel.Information);
    builder.Host.UseNLog();
    #endregion

    #region Dependency Injection
    builder.Services.AddScoped<IUnitOfWork>(sp =>
    {
        var dbContext = sp.GetRequiredService<PostgresDBContext>();
  return new UnitOfWork(dbContext);
    });

    builder.Services.AddScoped<IUserService, UserService>();
  builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IPermissionService, PermissionService>();
    #endregion

  #region App Configuration
    var app = builder.Build();

    // Ensure database created
    using (var scope = app.Services.CreateScope())
    {
  var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
      logger.Info("Database ensured created");
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

    // ?? MIDDLEWARE PIPELINE ORDER IS CRITICAL
    app.UseHttpsRedirection();
    app.UseStaticFiles();      // ? MUST be before UseRouting for CSS/JS

    app.UseRouting();

    // ? Remove Swagger (or comment out)
    // app.UseSwagger();
    // app.UseSwaggerUI(...);

    // ? Remove CORS (not needed for MVC)
    // app.UseCors(...);

    app.UseSession();    // ? BEFORE UseAuthentication
    app.UseAuthentication(); // ? Cookie Authentication
    app.UseAuthorization();

    // ? Use BaseNetCore middleware without Auth
    app.UseBaseNetCoreMiddleware();

    // ?? Comment out if not using
    // app.UseCoreDynamicPermissionMiddleware();

    // ? MVC Routes
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // ? Remove API route mapping
    // app.MapControllers();

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

### **B??c 6: Create MVC Controllers**

#### **6.1. HomeController.cs** (Thêm m?i)

```csharp
using Microsoft.AspNetCore.Mvc;
using BaseSourceImpl.Presentation.Models;
using System.Diagnostics;

namespace BaseSourceImpl.Presentation.Controllers
{
    public class HomeController : Controller
    {
private readonly ILogger<HomeController> _logger;

public HomeController(ILogger<HomeController> logger)
        {
     _logger = logger;
   }

        public IActionResult Index()
        {
  return View();
        }

        public IActionResult Privacy()
        {
return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
    {
          return View(new ErrorViewModel
            {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
      });
     }
    }
}
```

#### **6.2. AccountController.cs** (Thay th? AuthController)

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BaseSourceImpl.Application.Services.Auth;
using BaseSourceImpl.Application.Services.User;
using BaseSourceImpl.Presentation.Models.ViewModels;
using System.Security.Claims;

namespace BaseSourceImpl.Presentation.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

  public AccountController(
    IAuthService authService,
        IUserService userService,
            ILogger<AccountController> logger)
        {
  _authService = authService;
  _userService = userService;
          _logger = logger;
      }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            // If already authenticated, redirect
      if (User.Identity?.IsAuthenticated == true)
    {
    return RedirectToAction("Index", "Home");
        }

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
        // Validate user credentials using existing AuthService
            var user = await _userService.GetByUserNameAsync(model.UserName);
   
       if (user?.Value == null)
              {
         ModelState.AddModelError(string.Empty, "Tên ??ng nh?p ho?c m?t kh?u không ?úng.");
          return View(model);
                }

           // Verify password (assuming you have this in AuthService)
                // For now, calling existing login logic
       var loginRequest = new Presentation.Controllers.Auth.Models.LoginRequest
           {
          UserName = model.UserName,
             Password = model.Password
     };

       // This will throw exception if invalid
 var authResult = await _authService.Login(loginRequest);

    // Build claims from user data
  var claims = new List<Claim>
     {
             new Claim(ClaimTypes.NameIdentifier, user.Value.Id.ToString()),
     new Claim(ClaimTypes.Name, user.Value.UserName),
   new Claim(ClaimTypes.Email, user.Value.Email ?? string.Empty),
  new Claim("TypeAccount", user.Value.TypeAccount.ToString())
     };

              // Add roles
foreach (var roleId in user.Value.RoleIdList)
          {
           claims.Add(new Claim(ClaimTypes.Role, roleId.ToString()));
      }

   var claimsIdentity = new ClaimsIdentity(
       claims,
  CookieAuthenticationDefaults.AuthenticationScheme);

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
       ModelState.AddModelError(string.Empty, "Tên ??ng nh?p ho?c m?t kh?u không ?úng.");
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Unauthorized()
        {
         return View();
   }
    }
}
```

#### **6.3. UsersController.cs** (Chuy?n t? API sang MVC)

**Xóa file c?:** `Presentation/Controllers/User/UserController.cs`

**T?o file m?i:** `Presentation/Controllers/UsersController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using BaseSourceImpl.Application.Services.User;
using BaseSourceImpl.Application.DTOs.User;
using BaseSourceImpl.Presentation.Controllers.User.Models;
using BaseSourceImpl.Presentation.Models.ViewModels;

namespace BaseSourceImpl.Presentation.Controllers
{
    [Authorize] // Require authentication for all actions
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;

  public UsersController(
   IUserService userService,
            IMapper mapper,
         ILogger<UsersController> logger)
    {
     _userService = userService;
        _mapper = mapper;
    _logger = logger;
        }

        // GET: /Users
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] UserSearchModel searchModel)
        {
     try
            {
          var result = await _userService.GetPageAsync(searchModel);
      
           var viewModel = new UserListViewModel
        {
 Users = result.Items,
            TotalCount = result.TotalCount,
          PageNumber = result.PageNumber,
         PageSize = result.PageSize,
       SearchModel = searchModel
             };

          return View(viewModel);
         }
            catch (Exception ex)
  {
  _logger.LogError(ex, "Error loading users list");
         TempData["Error"] = "Có l?i x?y ra khi t?i danh sách ng??i dùng.";
                return View(new UserListViewModel());
  }
        }

        // GET: /Users/Details/5
     [HttpGet]
     public async Task<IActionResult> Details(int id)
        {
            try
  {
           var result = await _userService.GetByIdAsync(id);
    if (result?.Value == null)
    {
              return NotFound();
     }

      return View(result.Value);
       }
    catch (Exception ex)
       {
    _logger.LogError(ex, $"Error loading user details for id {id}");
         TempData["Error"] = "Không tìm th?y ng??i dùng.";
       return RedirectToAction(nameof(Index));
    }
        }

        // GET: /Users/Create
        [HttpGet]
  [Authorize(Roles = "1")] // Admin only (assuming role 1 is Admin)
        public IActionResult Create()
    {
 return View();
        }

      // POST: /Users/Create
        [HttpPost]
     [ValidateAntiForgeryToken]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> Create(CreateUserRequest model)
        {
            if (!ModelState.IsValid)
            {
       return View(model);
          }

            try
            {
   var dto = _mapper.Map<UserDto>(model);
 await _userService.CreateAsync(dto);

    TempData["Success"] = "T?o ng??i dùng thành công!";
              return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
      {
      _logger.LogError(ex, "Error creating user");
       ModelState.AddModelError(string.Empty, "Có l?i x?y ra khi t?o ng??i dùng.");
     return View(model);
            }
        }

  // GET: /Users/Edit/5
    [HttpGet]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> Edit(int id)
    {
      try
          {
     var result = await _userService.GetByIdAsync(id);
     if (result?.Value == null)
                {
        return NotFound();
            }

   var model = _mapper.Map<UpdateUserRequest>(result.Value);
   return View(model);
      }
            catch (Exception ex)
            {
     _logger.LogError(ex, $"Error loading user for edit, id {id}");
                TempData["Error"] = "Không tìm th?y ng??i dùng.";
     return RedirectToAction(nameof(Index));
      }
        }

        // POST: /Users/Edit/5
        [HttpPost]
    [ValidateAntiForgeryToken]
        [Authorize(Roles = "1")]
 public async Task<IActionResult> Edit(int id, UpdateUserRequest model)
  {
            if (!ModelState.IsValid)
 {
 return View(model);
        }

     try
            {
       var dto = _mapper.Map<UserDto>(model);
          dto.Id = id;
                await _userService.UpdateAsync(dto);

           TempData["Success"] = "C?p nh?t ng??i dùng thành công!";
 return RedirectToAction(nameof(Index));
   }
    catch (Exception ex)
    {
    _logger.LogError(ex, $"Error updating user, id {id}");
      ModelState.AddModelError(string.Empty, "Có l?i x?y ra khi c?p nh?t ng??i dùng.");
        return View(model);
            }
        }

        // GET: /Users/Delete/5
        [HttpGet]
 [Authorize(Roles = "1")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
         var result = await _userService.GetByIdAsync(id);
                if (result?.Value == null)
        {
           return NotFound();
   }

            return View(result.Value);
       }
            catch (Exception ex)
          {
      _logger.LogError(ex, $"Error loading user for delete, id {id}");
     TempData["Error"] = "Không tìm th?y ng??i dùng.";
    return RedirectToAction(nameof(Index));
}
        }

      // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
 [ValidateAntiForgeryToken]
        [Authorize(Roles = "1")]
   public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
  await _userService.DeleteAsync(id);
       TempData["Success"] = "Xóa ng??i dùng thành công!";
        return RedirectToAction(nameof(Index));
            }
     catch (Exception ex)
       {
        _logger.LogError(ex, $"Error deleting user, id {id}");
         TempData["Error"] = "Có l?i x?y ra khi xóa ng??i dùng.";
   return RedirectToAction(nameof(Index));
          }
      }
    }
}
```

---

### **B??c 7: Create ViewModels**

#### **7.1. ErrorViewModel.cs**

**T?o:** `Presentation/Models/ErrorViewModel.cs`

```csharp
namespace BaseSourceImpl.Presentation.Models
{
    public class ErrorViewModel
    {
     public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
public int? StatusCode { get; set; }
     public string? Message { get; set; }
    }
}
```

#### **7.2. LoginViewModel.cs**

**T?o:** `Presentation/Models/ViewModels/LoginViewModel.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace BaseSourceImpl.Presentation.Models.ViewModels
{
    public class LoginViewModel
    {
     [Required(ErrorMessage = "Tên ??ng nh?p là b?t bu?c")]
    [Display(Name = "Tên ??ng nh?p")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "M?t kh?u là b?t bu?c")]
        [DataType(DataType.Password)]
        [Display(Name = "M?t kh?u")]
    public string Password { get; set; } = string.Empty;

   [Display(Name = "Ghi nh? ??ng nh?p")]
        public bool RememberMe { get; set; }
    }
}
```

#### **7.3. UserListViewModel.cs**

**T?o:** `Presentation/Models/ViewModels/UserListViewModel.cs`

```csharp
using BaseSourceImpl.Application.DTOs.User;
using BaseSourceImpl.Presentation.Controllers.User.Models;

namespace BaseSourceImpl.Presentation.Models.ViewModels
{
    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new();
        public int TotalCount { get; set; }
      public int PageNumber { get; set; }
        public int PageSize { get; set; }
   public UserSearchModel? SearchModel { get; set; }

      public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
```

---

### **B??c 8: Create Razor Views**

Do h?n ch? ?? dài, tôi s? ch? cung c?p các view quan tr?ng nh?t. B?n có th? t?o ??y ?? b?ng scaffolding.

#### **8.1. _Layout.cshtml**

**T?o:** `Views/Shared/_Layout.cshtml`

```cshtml
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - BaseSourceImpl</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <header>
        <nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3">
       <div class="container-fluid">
    <a class="navbar-brand" asp-area="" asp-controller="Home" asp-action="Index">BaseSourceImpl</a>
     <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target=".navbar-collapse">
                <span class="navbar-toggler-icon"></span>
          </button>
        <div class="navbar-collapse collapse d-sm-inline-flex justify-content-between">
          <ul class="navbar-nav flex-grow-1">
      <li class="nav-item">
             <a class="nav-link text-dark" asp-controller="Home" asp-action="Index">Trang ch?</a>
          </li>
 @if (User.Identity?.IsAuthenticated == true)
    {
                <li class="nav-item">
   <a class="nav-link text-dark" asp-controller="Users" asp-action="Index">Ng??i dùng</a>
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
      @if (TempData["Success"] != null)
    {
        <div class="alert alert-success alert-dismissible fade show" role="alert">
       @TempData["Success"]
         <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
           </div>
          }
    @if (TempData["Error"] != null)
            {
             <div class="alert alert-danger alert-dismissible fade show" role="alert">
            @TempData["Error"]
           <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
          }
  @RenderBody()
     </main>
    </div>

  <footer class="border-top footer text-muted">
        <div class="container">
            &copy; 2024 - BaseSourceImpl - <a asp-area="" asp-controller="Home" asp-action="Privacy">Privacy</a>
        </div>
    </footer>

    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

#### **8.2. _LoginPartial.cshtml**

**T?o:** `Views/Shared/_LoginPartial.cshtml`

```cshtml
@using System.Security.Claims

@if (User.Identity?.IsAuthenticated == true)
{
    <ul class="navbar-nav">
        <li class="nav-item">
      <span class="navbar-text me-2">
        Xin chào, <strong>@User.Identity.Name</strong>
          </span>
        </li>
        <li class="nav-item">
    <form asp-controller="Account" asp-action="Logout" method="post" class="form-inline">
 <button type="submit" class="btn btn-link nav-link">??ng xu?t</button>
         </form>
        </li>
    </ul>
}
else
{
    <ul class="navbar-nav">
        <li class="nav-item">
            <a class="nav-link text-dark" asp-controller="Account" asp-action="Login">??ng nh?p</a>
        </li>
    </ul>
}
```

#### **8.3. Account/Login.cshtml**

**T?o:** `Views/Account/Login.cshtml`

```cshtml
@model BaseSourceImpl.Presentation.Models.ViewModels.LoginViewModel
@{
    ViewData["Title"] = "??ng nh?p";
}

<div class="row justify-content-center mt-5">
    <div class="col-md-4">
        <div class="card">
  <div class="card-body">
           <h2 class="card-title text-center">@ViewData["Title"]</h2>
                <hr />
          <form asp-action="Login" method="post">
        <div asp-validation-summary="ModelOnly" class="text-danger"></div>

          <div class="mb-3">
    <label asp-for="UserName" class="form-label"></label>
   <input asp-for="UserName" class="form-control" autofocus />
               <span asp-validation-for="UserName" class="text-danger"></span>
            </div>

    <div class="mb-3">
    <label asp-for="Password" class="form-label"></label>
      <input asp-for="Password" class="form-control" type="password" />
           <span asp-validation-for="Password" class="text-danger"></span>
         </div>

  <div class="mb-3 form-check">
          <input asp-for="RememberMe" class="form-check-input" />
       <label asp-for="RememberMe" class="form-check-label"></label>
    </div>

              <div class="d-grid">
    <button type="submit" class="btn btn-primary">??ng nh?p</button>
 </div>
          </form>
         </div>
        </div>
 </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

#### **8.4. Users/Index.cshtml**

**T?o:** `Views/Users/Index.cshtml`

```cshtml
@model BaseSourceImpl.Presentation.Models.ViewModels.UserListViewModel
@{
    ViewData["Title"] = "Danh sách ng??i dùng";
}

<div class="row">
    <div class="col-12">
        <h2>@ViewData["Title"]</h2>
        <hr />

        @if (User.IsInRole("1")) // Admin role
        {
  <p>
      <a asp-action="Create" class="btn btn-primary">T?o ng??i dùng m?i</a>
            </p>
        }

        <table class="table table-striped table-hover">
         <thead>
      <tr>
    <th>ID</th>
      <th>Tên ??ng nh?p</th>
        <th>H? tên</th>
         <th>Email</th>
        <th>S? ?i?n tho?i</th>
           <th>Lo?i tài kho?n</th>
         <th></th>
    </tr>
   </thead>
       <tbody>
     @if (Model.Users?.Any() == true)
   {
          @foreach (var user in Model.Users)
           {
            <tr>
      <td>@user.Id</td>
      <td>@user.UserName</td>
              <td>@user.Name</td>
          <td>@user.Email</td>
       <td>@user.Phone</td>
   <td>@user.TypeAccount</td>
 <td>
           <a asp-action="Details" asp-route-id="@user.Id" class="btn btn-sm btn-info">Chi ti?t</a>
           @if (User.IsInRole("1"))
          {
         <a asp-action="Edit" asp-route-id="@user.Id" class="btn btn-sm btn-warning">S?a</a>
   <a asp-action="Delete" asp-route-id="@user.Id" class="btn btn-sm btn-danger">Xóa</a>
      }
  </td>
            </tr>
        }
          }
       else
     {
   <tr>
                 <td colspan="7" class="text-center">Không có d? li?u</td>
                    </tr>
      }
            </tbody>
 </table>

@if (Model.TotalPages > 1)
        {
   <nav>
           <ul class="pagination">
           <li class="page-item @(!Model.HasPreviousPage ? "disabled" : "")">
      <a class="page-link" asp-action="Index" asp-route-pageNumber="@(Model.PageNumber - 1)">Tr??c</a>
         </li>
          @for (int i = 1; i <= Model.TotalPages; i++)
           {
             <li class="page-item @(i == Model.PageNumber ? "active" : "")">
        <a class="page-link" asp-action="Index" asp-route-pageNumber="@i">@i</a>
    </li>
         }
      <li class="page-item @(!Model.HasNextPage ? "disabled" : "")">
              <a class="page-link" asp-action="Index" asp-route-pageNumber="@(Model.PageNumber + 1)">Sau</a>
     </li>
     </ul>
            </nav>
        }
    </div>
</div>
```

---

### **B??c 9: Create wwwroot Structure**

```bash
# Create wwwroot folders
mkdir -p wwwroot/css
mkdir -p wwwroot/js
mkdir -p wwwroot/lib
mkdir -p wwwroot/images

# Download Bootstrap & jQuery (or use LibMan/npm)
# For quick setup, use CDN in _Layout.cshtml instead
```

**Minimal `wwwroot/css/site.css`:**

```css
html {
  font-size: 14px;
}

@media (min-width: 768px) {
  html {
    font-size: 16px;
  }
}

.btn:focus, .btn:active:focus, .btn-link.nav-link:focus, .form-control:focus, .form-check-input:focus {
  box-shadow: 0 0 0 0.1rem white, 0 0 0 0.25rem #258cfb;
}

html {
  position: relative;
  min-height: 100%;
}

body {
  margin-bottom: 60px;
}

.footer {
  position: absolute;
  bottom: 0;
  width: 100%;
  white-space: nowrap;
  line-height: 60px;
}
```

---

### **B??c 10: Build & Test**

```bash
# Clean solution
dotnet clean

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

**Verify:**
1. Navigate to `https://localhost:5001`
2. Should redirect to `/Home/Index`
3. Click "??ng nh?p" ? `/Account/Login`
4. Login with:
   - Username: `Admin`
   - Password: `123456`
5. Should see "Ng??i dùng" menu item
6. Navigate to `/Users` ? Should display user list
7. Test logout

---

## ? Testing & Verification

### **Test Cases:**

| Test Case | Expected Result | Status |
|-----------|----------------|--------|
| Access `/` without login | Redirect to Home | ? |
| Access `/Users` without login | Redirect to Login | ? |
| Login with valid credentials | Redirect to Home, show user name | ? |
| Login with invalid credentials | Show error message | ? |
| Access `/Users/Create` as non-admin | 403 Forbidden | ? |
| Access `/Users/Create` as admin | Show create form | ? |
| Create new user | Success message, redirect to list | ? |
| Edit user | Success message, updated data | ? |
| Delete user | Success message, removed from list | ? |
| Logout | Redirect to Home, show Login link | ? |
| CSRF attack | Form post fails without token | ? |

---

## ?? Rollback Plan

N?u có v?n ??, rollback v? API project:

```bash
# Discard all changes
git reset --hard HEAD

# Or restore specific commit
git checkout <commit-hash>

# Or merge back from backup branch
git checkout main
git merge --no-ff backup-before-mvc
```

---

## ?? So Sánh Chi Ti?t

### **Before (API) vs After (MVC)**

| Component | API Project | MVC Project |
|-----------|-------------|-------------|
| **Authentication** | JWT Bearer Token | Cookie-based |
| **Controllers** | `ControllerBase` + `[ApiController]` | `Controller` |
| **Return Types** | `IActionResult` with JSON | `IActionResult` with Views |
| **Authorization** | `Authorization: Bearer {token}` | Automatic cookie |
| **CSRF Protection** | ? Not needed | ? `[ValidateAntiForgeryToken]` |
| **Swagger** | ? Enabled | ? Removed |
| **Static Files** | ? Not used | ? wwwroot folder |
| **Views** | ? None | ? Razor views |
| **Session** | ? Stateless | ? Server-side |
| **Error Handling** | JSON response | Redirect to Error view |
| **CORS** | ? Required | ? Not needed |
| **Mobile Apps** | ? Perfect | ? Not suitable |
| **SEO** | ? Poor | ? Better |

---

## ?? Summary

### **What Changed:**
1. ? Authentication: JWT ? Cookie
2. ? Controllers: ApiController ? MVC Controller
3. ? Response: JSON ? HTML Views
4. ? Added: wwwroot, Views, ViewModels
5. ? Removed: Swagger, CORS, JWT-specific code
6. ? Updated: Program.cs, appsettings.json
7. ? Added: Session support, CSRF protection

### **What Stayed Same:**
1. ? Application layer (Services, DTOs)
2. ? Domain layer (Entities, DbContext)
3. ? Database & migrations
4. ? AutoMapper configurations
5. ? NLog logging
6. ? BaseNetCore.Core library features

### **When to Use:**
- **MVC**: Traditional web apps, internal tools, admin panels
- **API**: Mobile apps, SPAs, microservices, third-party integrations

---

## ?? Support

N?u g?p v?n ??:
1. Check logs in `logs/` folder (NLog)
2. Check browser console (F12)
3. Verify middleware order in Program.cs
4. Ensure `[ValidateAntiForgeryToken]` on POST actions

---

**Created by:** BaseSourceImpl Team  
**Version:** 1.0.0  
**Last Updated:** 2024-01-18  
**Repository:** https://github.com/HoangSonLe/BaseCoreNetCoreNugetImpl
