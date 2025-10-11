using CampusEvents.Data;
using CampusEvents.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
//add controller
builder.Services.AddControllersWithViews();
// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add DbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

IdentityBuilder identityBuilder = builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Diagnostic: try resolving the identity stores to get the underlying error
builder.Services.AddLogging(); // ensure logging available

var preProvider = builder.Services.BuildServiceProvider(validateScopes: true);
try
{
    // Attempt to resolve the stores that Identity needs
    var userStore = preProvider.GetService<Microsoft.AspNetCore.Identity.IUserStore<User>>();
    var roleStore = preProvider.GetService<Microsoft.AspNetCore.Identity.IRoleStore<IdentityRole>>();

    Console.WriteLine($"Diagnostic: IUserStore<User> resolved? {userStore != null}");
    Console.WriteLine($"Diagnostic: IRoleStore<IdentityRole> resolved? {roleStore != null}");
}
catch (Exception ex)
{
    Console.WriteLine("Diagnostic exception resolving services:");
    Console.WriteLine(ex.ToString());
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapRazorPages();
app.Run();
