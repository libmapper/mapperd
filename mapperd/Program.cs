using System.Diagnostics;
using IdGen;
using mapperd.Model;
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

        builder.Services.AddControllers();

        // Identifier generator
        builder.Services.AddSingleton(new IdGenerator(Environment.ProcessId % 1024));
        builder.Services.AddSingleton<ConnectionManager>();

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