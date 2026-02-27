using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniCMS.Web.Data;
using MiniCMS.Web.Services; // adjust if your namespaces differ

var builder = WebApplication.CreateBuilder(args);

// =========================
// Existing services (keep)
// =========================

// DB context registration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (as per your Source of Truth)
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC controllers + API controllers (keep)
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

// ✅ Milestone 6: enable Razor Pages so Identity UI endpoints exist
builder.Services.AddRazorPages();

// Swagger (keep existing behavior)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IHtmlSanitizationService, HtmlSanitizationService>();

var app = builder.Build();

// =========================
// Existing middleware (keep)
// =========================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Keep your existing /dev/sanitize endpoint behavior in Development.
    // (If you already have this defined elsewhere, keep yours and remove this duplicate.)
    // app.MapGet("/dev/sanitize", () => Results.Ok("..."));
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Required for cookie login + [Authorize]
app.UseAuthentication();
app.UseAuthorization();

// =========================
// Existing seeding (keep)
// =========================

// Keep your existing seeding call exactly as-is if it already exists.
// This is a common pattern:
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbSeeder.SeedAsync(services);
}

// =========================
// Endpoint mapping (updated)
// =========================

// Optional convenience: /Admin -> /Admin/Home/Index
// app.MapGet("/Admin", context =>
// {
//     context.Response.Redirect("/Admin/Home/Index");
//     return Task.CompletedTask;
// });

// Area route FIRST (so /Admin/... resolves)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Keep your existing default MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Keep your existing API mapping
app.MapControllers();

// Milestone 6: Identity UI endpoints (e.g. /Identity/Account/Login)
app.MapRazorPages();

app.Run();