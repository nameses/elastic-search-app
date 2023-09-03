using elastic_search_app.Entities;
using elastic_search_app.Services;
using elastic_search_app.Settings.Configuration;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsecrets.json")
    .Build();
builder.Services.Configure<ElasticConfig>(configuration.GetSection("Elasticsearch"));
//DbContext
var connString = configuration.GetConnectionString("SqlConnection");
builder.Services.AddDbContext<AppDbContext>(option => option.UseSqlServer(connString));
//hangfire
builder.Services.AddHangfire(c => c
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));
builder.Services.AddHangfireServer();

builder.Services.AddControllersWithViews();

//services
builder.Services.AddScoped<DataGeneratorService>();
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<SyncService>();
//log, serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseHangfireDashboard();

app.UseSerilogRequestLogging();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapHangfireDashboard();
app.MapFallbackToFile("index.html");

//hangfire reccurent tasks
var manager = new RecurringJobManager();
manager.AddOrUpdate<SyncService>("sync-mssql-and-elasticsearch", x => x.SyncData(), Cron.Minutely());

app.Run();
