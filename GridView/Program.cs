using GridView.CoreServiceProviders;
using GridView.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register HttpContextAccessor so we can access HttpContext statically
builder.Services.AddHttpContextAccessor();

// Allow synchronous IO
builder.WebHost.ConfigureKestrel(options =>
{
    options.AllowSynchronousIO = true;
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseSqlServer("Server=SAP-16;Database=GridViewSampleByCodeFirst;User ID=sa;Password=137011;TrustServerCertificate=True;"));

// حالا Build
var app = builder.Build();

// Save IServiceProvider globally
CoreServiceProviders.serviceProvider = app.Services;

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index_GridWith_Filter_Sort_Paging_Grouping}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
    SeedData.Initialize(context);
}



app.Run();
