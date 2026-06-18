using Application.Common.Interfaces.Persistence;
using Application.Payments.Commands.CreatePayment;
using Application.Payments.Idempotency;
using Application.Payments.Outbox;
using Application.Payments.Queries.GetPayment;
using Application.Payments.Queries.SearchPayments;
using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Persistence.Entities;
using Persistence.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//DB CONTEXT
var conn = builder.Configuration.GetConnectionString("AppConnection");
builder.Services.AddDbContext<PaymentProcessorContext>(x => x.UseMySQL(conn));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<CreatePaymentHandler, CreatePaymentHandler>();
builder.Services.AddScoped<GetPaymentHandler, GetPaymentHandler>();
builder.Services.AddScoped<SearchPaymentHandler, SearchPaymentHandler>();
builder.Services.AddScoped<IIdempotencyStore, IdempotencyStore>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IMerchantRepository, MerchantRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITransactionEventRepository, TransactionEventRepository>();
builder.Services.AddScoped<IReconciliationQueueRepository, ReconciliationQueueRepository>();


builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext());

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;

        if (exception is DomainException domainException)
        {
            context.Response.StatusCode = domainException.StatusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                error = domainException.Code,
                message = domainException.Message
            });

            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "internal_error",
            message = "An unexpected error occurred."
        });
    });
});

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
