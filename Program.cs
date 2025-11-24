using Microsoft.EntityFrameworkCore;
using CampusEvents.Data;
using CampusEvents.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();

// Add session support for user authentication and state management
// Session timeout set to 30 minutes of inactivity
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true; // Prevents JavaScript access for security
    options.Cookie.IsEssential = true; // Required for GDPR compliance
});

// Add DbContext with SQLite database
// Uses connection string from appsettings.json or defaults to campusevents.db
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=campusevents.db"));

// Register CSV data communication service (transient - new instance per request)
builder.Services.AddTransient<DbCSVCommunicator>();

// Register US.04 services (Carpool and Room Rental system)
// Scoped services - one instance per HTTP request
builder.Services.AddScoped<CarpoolService>();
builder.Services.AddScoped<RoomRentalService>();
builder.Services.AddScoped<ProximityService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbCSV = services.GetRequiredService<DbCSVCommunicator>();
    dbCSV.Test();
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();