using Bison.Razor.Data;
using Microsoft.EntityFrameworkCore;
using Bison.Razor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IObservationService, ObservationService>();

// Allow the SQLite path to be overridden via the BISONDBPATH environment
// variable (tests use this to point the app at a hermetic temp database) or
// the BISONDBPATH configuration key. The default is the project's tracked
// Data/bison.db, relative to the working directory (the app is run from
// src/Bison.Razor, where Data/bison.db lives).
var bisonDbPath =
    Environment.GetEnvironmentVariable("BISONDBPATH")
    ?? builder.Configuration["BISONDBPATH"]
    ?? "Data/bison.db";
builder.Services.AddDbContext<BisonDBContext>(options =>
    options.UseSqlite($"Data Source={bisonDbPath}"));

var app = builder.Build();

// Ensure the database is created. Only seed when BISON_SEED is not set to
// "false" so tests that pre-seed their own data are not clobbered by the
// app's 400+ post seed.
using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider.GetRequiredService<BisonDBContext>();

    await context.Database.EnsureCreatedAsync();

    var seedEnabled = !string.Equals(
        builder.Configuration["BISON_SEED"], "false",
        StringComparison.OrdinalIgnoreCase);
    if (seedEnabled)
    {
        DbInitializer.SeedDatabase(context);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();
