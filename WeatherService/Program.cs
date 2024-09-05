using WeatherService.Services;
using WeatherService.Models;
using Microsoft.OpenApi.Models;
using Serilog;
using WeatherService.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Set up Serilog for logging
// This reads the config from appsettings.json and creates a logger
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// Tell our app to use Serilog for logging
builder.Host.UseSerilog();

// Register services for dependency injection
builder.Services.AddControllers();
builder.Services.AddHttpClient();
// Grab database settings from config
builder.Services.Configure<WeatherDatabaseSettings>(builder.Configuration.GetSection("WeatherDatabase"));
// Register our custom services
builder.Services.AddScoped<IWeatherService, WeatherService.Services.WeatherService>();
builder.Services.AddScoped<IGeocodingService, GeocodingService>();

// Set up Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Weather Service API", Version = "v1" });
});

var app = builder.Build();

// If we are in development show swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Force HTTPS
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Log all HTTP requests
app.UseSerilogRequestLogging();

// Fire up the app
app.Run();