//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using API.Middleware;
using API.Routes;
using Application.Services;
using Application.Validators;
using Domain.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Repository.Data;
using Repository.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var pgBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = "localhost",
        Port = 5432,
        Database = "tinosnegocios",
        Username = "postgres",
        Password = "1q2w3e4r@#$", //send to env file
        SslMode = SslMode.Disable,
        Pooling = true,
        Timeout = 15
    };

    //string connectionString = builder.Configuration.GetConnectionString(pgBuilder.ConnectionString) ?? string.Empty;
    options.UseNpgsql(pgBuilder.ConnectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(3);
        npgsqlOptions.CommandTimeout(30);
        npgsqlOptions.MinBatchSize(1);
        npgsqlOptions.MaxBatchSize(100);
    });
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
});

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IExpenseItemRepository, ExpenseItemRepository>();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IIncomeService, IncomeService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IExpenseItemService, ExpenseItemService>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateExpenseRequestValidator>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandlerMiddleware();
app.UseResponseWrapperMiddleware();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapCategoryRoutes();
app.MapAccountRoutes();
app.MapExpenseRoutes();
app.MapIncomeRoutes();
app.MapBudgetRoutes();
app.MapExpenseItemRoutes();

app.Run();

public partial class Program { }
