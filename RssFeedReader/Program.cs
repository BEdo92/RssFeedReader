using Microsoft.EntityFrameworkCore;
using RssFeedReader.Data;
using RssFeedReader.Extensions;
using RssFeedReader.Interfaces;
using RssFeedReader.Middleware;
using RssFeedReader.Repositories;
using RssFeedReader.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddDbContext<FeedContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<RssFeedService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseStaticFiles();

//app.UseCors(x => x
//    .AllowAnyHeader()
//    .AllowAnyMethod()
//    .WithOrigins("https://localhost:7170"));

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using IServiceScope scope = app.Services.CreateScope();
IServiceProvider services = scope.ServiceProvider;
try
{
    FeedContext context = services.GetRequiredService<FeedContext>();
    await Seed.SeedFeedCources(context);
    await context.SaveChangesAsync();
}
catch (Exception ex)
{
    ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration.");
}

app.Run();
