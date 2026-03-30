using ExchangeRateManagement.Repository.Abstractions;
using ExchangeRateManagement.Repository.ExchangeRateManagement;
using ExchangeRateManagement.Service.Abstractions;
using ExchangeRateManagement.Service.ExchangeRateProvider;
using ExchangeRateManagement.Service.ExchangeRateService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IExchangeRateRepository, InMemoryExchangeRateRepository>();
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
builder.Services.AddHttpClient<IExchangeRateProvider, ExchangeRateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
