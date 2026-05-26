var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Insert(0, "/theme/default/frontend/{1}/{0}.cshtml");
        options.ViewLocationFormats.Insert(1, "/theme/default/frontend/Shared/{0}.cshtml");
    });

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = OntoCms.Conventions.Auth.StaffAuthenticationHandler.CookieSchemeName;
        options.DefaultChallengeScheme = OntoCms.Conventions.Auth.StaffAuthenticationHandler.CookieSchemeName;
        options.DefaultSignInScheme = OntoCms.Conventions.Auth.StaffAuthenticationHandler.CookieSchemeName;
    })
    .AddCookie(OntoCms.Conventions.Auth.StaffAuthenticationHandler.CookieSchemeName, options =>
    {
        options.Cookie.Name = OntoCms.Conventions.Auth.StaffAuthenticationHandler.CookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    })
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, OntoCms.Conventions.Auth.StaffAuthenticationHandler>(
        OntoCms.Conventions.Auth.StaffAuthenticationHandler.SchemeName,
        static _ => { });

builder.Services.AddScoped<OntoCms.Modules.Option.OptionFeed>();
builder.Services.AddScoped<OntoCms.Modules.Post.PostFeed>();
builder.Services.AddScoped<OntoCms.Modules.Role.RoleFeed>();
builder.Services.AddScoped<OntoCms.Modules.Staff.StaffFeed>();
builder.Services.AddScoped<OntoCms.Conventions.Auth.StaffClaimsPrincipalFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
