using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization.Metadata;
using Server.Data;
using Server.Services.SongServices;
using Server.Services.GameServices;
using Server.Services.Factories;
using Npgsql;

var builder = WebApplication.CreateBuilder(args); 

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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
