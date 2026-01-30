using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using TourismWebsite.Data;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);


// 🔹 FORCE AUD CULTURE (put it HERE)
var cultureInfo = new CultureInfo("en-AU");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity with Roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // simplify registration
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddSingleton<IEmailSender, TourismWebsite.Services.EmailSender>(); // <<< THIS FIXES REGISTER
builder.Services.AddControllersWithViews();


// Configure paths for login & access denied
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();




// Seed Roles & Default Users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Admin", "Tourist" };
    foreach (var role in roles)
    {
        if (!roleManager.RoleExistsAsync(role).Result)
            roleManager.CreateAsync(new IdentityRole(role)).Wait();
    }

    // Default Admin
    string adminEmail = "admin@tourism.com";
    if (userManager.FindByEmailAsync(adminEmail).Result == null)
    {
        var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail };
        userManager.CreateAsync(admin, "Admin123!").Wait();
        userManager.AddToRoleAsync(admin, "Admin").Wait();
    }

    // Default Tourist
    string touristEmail = "tourist@tourism.com";
    if (userManager.FindByEmailAsync(touristEmail).Result == null)
    {
        var tourist = new IdentityUser { UserName = touristEmail, Email = touristEmail };
        userManager.CreateAsync(tourist, "Tourist123!").Wait();
        userManager.AddToRoleAsync(tourist, "Tourist").Wait();
    }
}

// Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages for Identity
app.MapRazorPages();

app.Run();
