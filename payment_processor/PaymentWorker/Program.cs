using Application.Common.Interfaces.Persistence;
using Application.Integrations.Acquirers;
using Application.Payments.Outbox;
using Domain.Interfaces;
using Integrations.Acquirers;
using Microsoft.EntityFrameworkCore;
using PaymentWorker.Services;
using Persistence.Entities;
using Persistence.Repositories;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Services.AddSerilog();

var conn = builder.Configuration.GetConnectionString("AppConnection");
builder.Services.AddDbContext<PaymentProcessorContext>(x => x.UseMySQL(conn));

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<ITransactionEventRepository, TransactionEventRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IReconciliationQueueRepository, ReconciliationQueueRepository>();
builder.Services.AddScoped<PaymentAuthorizationProcessor>();
builder.Services.AddHostedService<PaymentAuthorizationWorker>();
builder.Services.AddHttpClient<IAcquirerGateway, HttpAcquirerGateway>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Acquirer:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});

var host = builder.Build();
await host.RunAsync();