using System.Reflection;
using Microsoft.OpenApi;
using TicTacToe.Api.ErrorHandling;
using TicTacToe.Application;
using TicTacToe.Infrastructure;

const string AngularDevCorsPolicy = "AngularDev";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tic-Tac-Toe API",
        Version = "v1",
        Description = "REST API for the Tic-Tac-Toe game: game lifecycle, moves, undo, reset, and the running scoreboard."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Repositories are the only in-memory state in the app; one instance per process.
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();
builder.Services.AddSingleton<IScoreboardRepository, InMemoryScoreboardRepository>();

builder.Services.AddScoped<ComputerPlayerService>();
builder.Services.AddScoped<ScoreboardService>();
builder.Services.AddScoped<GameService>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Must run first so exceptions from any later middleware are still caught.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tic-Tac-Toe API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors(AngularDevCorsPolicy);
app.MapControllers();

app.Run();
