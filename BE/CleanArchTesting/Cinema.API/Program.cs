
using Application.Abstractions.Config;
using Application.Abstractions.Payments;
using Application.Abstractions.Persistence;
using Application.Abstractions.Pricing;
using Application.Abstractions.Time;
using Infrastructure.Background;
using Infrastructure.Data;
using Infrastructure.Implementations.Payments;
using Infrastructure.Implementations.Time;

namespace Cinema.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if ( app.Environment.IsDevelopment() )
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
