using Bison.Razor.Data;
using Bison.Razor.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("BisonDBContext") ?? throw new InvalidOperationException("Connection string 'BisonDBContext' not found.");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IObservationService, ObservationService>();
builder.Services.AddSingleton<IObservationTaxonFilterService, ObservationTaxonFilterService>();


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
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
})
.AddEntityFrameworkStores<BisonDBContext>();


builder.Services.AddSession();

var githubClientId = builder.Configuration["Authentication:Github:ClientId"];
var githubClientSecret = builder.Configuration["Authentication:Github:ClientSecret"];
var githubConfigured =
    !string.IsNullOrWhiteSpace(githubClientId)
    && !string.IsNullOrWhiteSpace(githubClientSecret);

var authentication = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    // Point the default challenge at GitHub only when it is actually
    // registered; otherwise the application cookie handles sign-in.
    options.DefaultChallengeScheme = githubConfigured
        ? "GitHub"
        : IdentityConstants.ApplicationScheme;
});

// GitHub OAuth is an optional external provider: register it only when real
// credentials are configured (local development via User Secrets, or a
// deployment with a provisioned GitHub OAuth app). In environments without
// credentials -- e.g. CI, which runs the WebApplicationFactory test host in
// Production and has no User Secrets -- the app still starts and the public
// timeline pages work, instead of throwing on every request while the
// auth middleware resolves the GitHub scheme's options.
if (githubConfigured)
{
    authentication.AddGitHub(o =>
    {
        o.ClientId = githubClientId!;
        o.ClientSecret = githubClientSecret!;
        o.CallbackPath = "/signin-github";
        o.SignInScheme = IdentityConstants.ExternalScheme;
    });
}

var app = builder.Build();

// Ensure the database is created. Only seed when BISON_SEED is not set to
// "false" so tests that pre-seed their own data are not clobbered by the
// app's 400+ post seed.
using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider.GetRequiredService<BisonDBContext>();

    // SQLite cannot create the parent directory of the DB file, so make sure
    // it exists before EnsureCreated. This lets the app run from any CWD
    // (e.g. a publish/ folder where Data/ has not been pre-created).
    var dbDir = Path.GetDirectoryName(Path.GetFullPath(bisonDbPath));
    if (!string.IsNullOrEmpty(dbDir))
    {
        Directory.CreateDirectory(dbDir);
    }

    await context.Database.EnsureCreatedAsync();

    var seedEnabled = !string.Equals(
    builder.Configuration["BISON_SEED"], "false",
    StringComparison.OrdinalIgnoreCase);
    if (seedEnabled)
    {
        DbInitializer.SeedDatabase(context);
        // Seed two users for testing purposes. The passwords are intentionally weak
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();
        await SeedUserAsync(userManager, "Eduard", "edka@itu.dk", "123456");
        await SeedUserAsync(userManager, "Debasmita", "debloh@itu.dk", "password");
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

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

app.Run();

// Seed a user with the given full name, email, and password if they don't already exist.
static async Task SeedUserAsync(
    UserManager<ApplicationUser> userManager,
    string fullName, string email, string password)
{
    if (await userManager.FindByEmailAsync(email) != null) return; 

    var user = new ApplicationUser
    {
        UserName = email,
        Email = email,
        EmailConfirmed = true   
    };

    var result = await userManager.CreateAsync(user, password);
    if (!result.Succeeded)
    {
        throw new InvalidOperationException(
            $"Could not seed {email}: " +
            string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    
    await userManager.AddClaimAsync(user, new Claim("FullName", fullName));
}
