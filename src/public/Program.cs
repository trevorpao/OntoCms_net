var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Insert(0, "/theme/default/frontend/{1}/{0}.cshtml");
        options.ViewLocationFormats.Insert(1, "/theme/default/frontend/Shared/{0}.cshtml");
    });

builder.Services.AddScoped<OntoCms.Modules.Option.OptionFeed>();
builder.Services.AddScoped<OntoCms.Modules.Post.PostFeed>();
builder.Services.AddScoped<OntoCms.Modules.Role.RoleFeed>();

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

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
