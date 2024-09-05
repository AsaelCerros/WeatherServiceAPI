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

Log.Information("Starting up the Weather Service API");

// Register services for dependency injection
builder.Services.AddControllers();
builder.Services.AddHttpClient();
// Grab database settings from config
builder.Services.Configure<WeatherDatabaseSettings>(builder.Configuration.GetSection("WeatherDatabase"));
// Register our custom services
builder.Services.AddScoped<IWeatherService, WeatherService.Services.WeatherService>();
builder.Services.AddScoped<IGeocodingService, GeocodingService>();

Log.Information("Configured services and dependency injection");

// Set up Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Weather Service API", Version = "v1" });
});

Log.Information("Configured Swagger for API documentation");

var app = builder.Build();

// If we are in development show swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    Log.Information("Development environment detected. Swagger UI enabled.");
}
else
{
    Log.Information("Production environment detected. Swagger UI disabled.");
}

// Force HTTPS
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Log all HTTP requests
app.UseSerilogRequestLogging();

Log.Information("Weather Service API is configured and ready to start");

// Fire up the app
try
{
    Log.Information("Starting Weather Service API");
    app.Run();
    Log.Information("Weather Service API stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Weather Service API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}