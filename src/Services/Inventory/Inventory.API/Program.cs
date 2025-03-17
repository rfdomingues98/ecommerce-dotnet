using Inventory.API.Data;
using Inventory.API.Services;
using Inventory.API.Services.Caching;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on port 80
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(80); // Listen for requests on port 80 from any IP
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database context
builder.Services.AddDbContext<InventoryContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("InventoryDB")));

// Register services
builder.Services.AddSingleton<IEventProducer, KafkaProducer>();
builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Add health checks
builder.Services.AddHealthChecks().AddDbContextCheck<InventoryContext>();

// Add Redis health check only if configured
if (!string.IsNullOrEmpty(builder.Configuration["Redis:ConnectionString"]))
{
    builder.Services.AddHealthChecks()
        .AddRedis(builder.Configuration["Redis:ConnectionString"]!);
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Add health check endpoint
app.MapHealthChecks("/health");
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
