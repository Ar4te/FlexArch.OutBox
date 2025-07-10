
using FlexArch.OutBox.Publisher.RabbitMQ;
using FlexArch.OutBox.Core;
using FlexArch.OutBox.Core.Middlewares;
using FlexArch.OutBox.Persistence.EFCore;
using Microsoft.EntityFrameworkCore;

namespace FlexArch.OutBox.TestAPI;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services
            .AddOutbox(outboxOptions =>
            {
                outboxOptions.ProcessingInterval = TimeSpan.FromSeconds(5);
                outboxOptions.BatchSize = 50;
                outboxOptions.EnableVerboseLogging = builder.Environment.IsDevelopment();
            })
            .AddEfPersistence<DbContext>()
            .AddRabbitMqOutbox(factory =>
            {
                // RabbitMQ连接配置
            })
            .WithDelay()
            .WithTracing()
            .WithRetry(cfg =>
            {
                cfg.MaxRetryCount = 10;
                cfg.DelayInSeconds = 3;
            })
            .WithMetrics()
            .WithMessageSigning(cfg =>
            {
                // 从配置读取密钥，而不是硬编码
                cfg.SecretKey = builder.Configuration["OutBox:SigningKey"] ?? "default-dev-key-change-in-production-min-32-chars";
                cfg.EnableSigning = true;
            })
            .WithCriticalBreaker(cfg =>
            {
                cfg.DurationOfBreakInSeconds = 10;
                cfg.FailureThreshold = 10;
            })
            .AddOutboxHealthChecks();

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
