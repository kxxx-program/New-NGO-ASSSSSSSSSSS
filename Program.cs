global using Demo;
global using Demo.Models;
using NGO_Web_Demo;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSqlServer<DB>($@"
    Data Source=(LocalDB)\MSSQLLocalDB;
    AttachDbFilename={builder.Environment.ContentRootPath}\DB.mdf;
");

builder.Services.AddScoped<Helper>();
builder.Services.AddHttpClient();
// Session Configuration - Add this for timeout functionality
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15); // 15-minute session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Authentication and Authorization with Enhanced Cookie Configuration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(15); // Match session timeout
        options.SlidingExpiration = true; // Refresh timeout on activity
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";

        // Security settings
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Name = "NGO.Auth"; // Custom cookie name

        // Optional: Configure cookie behavior
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                // Optional: Add custom validation logic here
                // For example, check if user still exists in database
                var userEmail = context.Principal?.Identity?.Name;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    // You can add database checks here if needed
                    // var db = context.HttpContext.RequestServices.GetRequiredService<DB>();
                    // var user = db.Users.Find(userEmail);
                    // if (user == null) context.RejectPrincipal();
                }
                await Task.CompletedTask;
            }
        };
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting(); // Add explicit routing

// IMPORTANT: Order matters here
app.UseSession();        // Session MUST come before Authentication
app.UseAuthentication(); // Authentication MUST come before Authorization  
app.UseAuthorization();

// Map routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();