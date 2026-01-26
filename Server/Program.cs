using Microsoft.EntityFrameworkCore; 
using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization.Metadata;
using Server.Data; 
using Server.Hubs;
using Server.Services.SongServices;
using Server.Services.GameServices;
using Server.Services.Factories;
using Npgsql;

var builder = WebApplication.CreateBuilder(args); 

// Add CORS policy to allow React app access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // allow cookies/auth headers 
    });
});

// Configure database connection 
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Setup Npgsql DataSource with JSON support
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson(); // enable JSON support 
var dataSource = dataSourceBuilder.Build(); 

// Add services to the container that will use the DataSource
builder.Services.AddDbContext<AppDbContext>(options => 
{
    options.UseNpgsql(dataSource);
}); 

// Register App Services
builder.Services.AddScoped<LocalSongService>();
builder.Services.AddScoped<SpotifySongService>();
builder.Services.AddScoped<SongServiceFactory>();
builder.Services.AddScoped<LocalGameService>();
builder.Services.AddScoped<OnlineGameService>();
builder.Services.AddScoped<GameServiceFactory>(); 

// SignalR Services
builder.Services.AddSignalR();

// Add App Controllers 
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // this ensures that derived types are properly serialized/deserialized
        options.JsonSerializerOptions.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
    });
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

// Middleware setup
app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers(); 

// Map SignalR hubs
app.MapHub<GameHub>("/gameHub");

// Run Server
app.Run();
