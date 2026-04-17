using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NFe.Infrastructure.Data;
using NFe.Application.Interfaces;
using NFe.Infrastructure.ExternalServices;
using NFe.Application.Services;
using IAuthService = NFe.Application.Interfaces.IAuthService;
using INfeService = NFe.Application.Interfaces.INfeService;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentUserName()
    .CreateLogger();

try
{
    Log.Information("Iniciando NFe/NFCe REST API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var secretKeyString = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is not configured");
    var secretKey = Encoding.ASCII.GetBytes(secretKeyString);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    builder.Services.AddDbContext<NfeDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IAuthService, NFe.Application.Services.AuthService>();
    builder.Services.AddScoped<INfeService, NFe.Application.Services.NfeService>();
    builder.Services.AddScoped<NFe.Application.Interfaces.INfceService, NFe.Application.Services.NfceService>();
    builder.Services.AddScoped<NFe.Infrastructure.ExternalServices.ISefazService, NFe.Infrastructure.ExternalServices.SefazService>();
    builder.Services.AddScoped<NFe.Infrastructure.ExternalServices.ICertificateService, NFe.Infrastructure.ExternalServices.CertificateService>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "NFe/NFCe REST API",
            Version = "v1",
            Description = "API para emissão de Notas Fiscais com integração SEFAZ Goiás"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                new string[] { }
            }
        });
    });

    builder.Services.AddControllers();

    builder.Services.AddHealthChecks();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseCors("AllowAll");

    app.MapHealthChecks("/health");
    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<NfeDbContext>();
        dbContext.Database.Migrate();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação terminada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}