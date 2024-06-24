using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using IdGen;
using Mapper;
using mapperd.Jobs;
using mapperd.Model;
using mapperd.Routes;
using mapperd.Util;
using NanoidDotNet;

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
        builder.Services.AddSwaggerGen(setupAction: (opt) =>
        {
            opt.OperationFilter<SessionIDParameterAdder>();
        });
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.TypeInfoResolver = new RequirePropertiesResolver();
            });

        // Identifier generator)
        builder.Services.AddSingleton<ConnectionManager>();
        builder.Services.AddSingleton(new Graph());
        builder.Services.AddSingleton(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
        
        builder.Services.AddHostedService<WebsocketJob>();
        builder.Services.AddHostedService<PollJob>();

        builder.Services.AddCors((options) =>
        {
            options.AddDefaultPolicy(opts =>
            {
                opts.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
        });
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors();
        
        app.UseWebSockets();
        
       // app.UseHttpsRedirection();
       
       app.UseMiddleware<SessionLookupMiddleware>();

        app.UseAuthorization();
        
        app.MapControllers();

        app.Run();
    }
}