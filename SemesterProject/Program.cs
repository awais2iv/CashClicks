using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<CategoryServices>();
builder.Services.AddSingleton<FeedbackServices>();
builder.Services.AddSingleton<NotesServices>();
builder.Services.AddSingleton<AccountServices>();
builder.Services.AddSingleton<UserState>();
builder.Services.AddSingleton<IncomeServices>();
builder.Services.AddSingleton<ExpenseServices>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
