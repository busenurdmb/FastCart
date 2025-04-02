using FastCart.Application.Interfaces;
using FastCart.Application.Settings;
using FastCart.Infrastructure.Pipeline.Cache.Redis;
using FastCart.Infrastructure.Services.Carts;
using Serilog.Events;
using Serilog;
using StackExchange.Redis;
using Serilog.Filters;
using FastCart.Infrastructure.Logging;
using Serilog.Sinks.Elasticsearch;
using FastCart.Infrastructure.Pipeline.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);


// Serilog yapılandırması
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()

    // 1. Genel log dosyası
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)

    // 2. CartService için filtrelenmiş özel log dosyası
    // CartService'e özel: Hem dosya hem Elasticsearch
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.FromSource("FastCart.Infrastructure.Services.Carts.CartService"))
        .WriteTo.File("Logs/Carts/cartlog-.txt", rollingInterval: RollingInterval.Day)
        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
        {
            AutoRegisterTemplate = true,
            IndexFormat = "fastcart-logs-{0:yyyy.MM.dd}",
            CustomFormatter = new SimpleElasticsearchFormatter()
        }))

    
    .CreateLogger();




// 🔽 Serilog'u appsettings.json üzerinden oku
//Log.Logger = new LoggerConfiguration()
//    .ReadFrom.Configuration(builder.Configuration)
//    .CreateLogger();

builder.Host.UseSerilog(); // Serilog'u host'a bağla


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.Configure<RedisSettings>(
    builder.Configuration.GetSection("RedisSettings")
);


// 2. IRedisService elle yapılandır
builder.Services.AddSingleton<IRedisService>(sp =>
{
    var config = builder.Configuration.GetSection("RedisSettings");
    var host = config.GetValue<string>("Host");
    var port = config.GetValue<int>("Port");

    var redisService = new RedisConnectionService(host, port);
    redisService.Connect();
    return redisService;
});

// 1. Redis ayarlarını config üzerinden al
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetSection("RedisSettings");
    var host = config.GetValue<string>("Host");
    var port = config.GetValue<int>("Port");
    return ConnectionMultiplexer.Connect($"{host}:{port}");
});

builder.Services.AddScoped<CartService>(); // ICacheService kullanan
builder.Services.AddScoped<DirectRedisCartService>(); // IRedisService kullanan
//builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IElasticService, ElasticService>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

//builder.Services.AddScoped<ICartService, CartService>();

var app = builder.Build();



Log.Information("🚀 Uygulama başlatıldı");

// ?? Swagger arayüzünü aktif et
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.MapControllers(); 

app.Run();


