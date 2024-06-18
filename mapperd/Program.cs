using System.Diagnostics;
using System.Text.Json;
using IdGen;
using mapperd.Model;
using mapperd.Routes;
using mapperd.Util;

namespace mapperd;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                options.JsonSerializerOptions.WriteIndented = true;
            });

        // Identifier generator
        builder.Services.AddSingleton(new IdGenerator(Environment.ProcessId % 1024));
        builder.Services.AddSingleton<ConnectionManager>();
        builder.Services.AddHostedService<WebsocketJob>();
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseWebSockets();
        
       // app.UseHttpsRedirection();
       
       app.UseMiddleware<SnowflakeLookupMiddleware>();

        app.UseAuthorization();
        
        app.MapControllers();

        app.Run();
    }
}