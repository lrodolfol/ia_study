//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using API.Middleware;
using Application.Services;
using Domain.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Repository.Data;
using Repository.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Application.Validators.CreateCategoryRequestValidator>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IExpenseItemRepository, ExpenseItemRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IIncomeService, IncomeService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IExpenseItemService, ExpenseItemService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<ResponseWrapperMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
