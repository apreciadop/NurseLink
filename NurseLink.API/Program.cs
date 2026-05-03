using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NurseLink.API.Database;
using NurseLink.API.Domain.Common;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NurseLink API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter only the JWT token, without Bearer.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});

// as we are developing, we are going to allow all the origins
builder.Services.AddCors(options =>
{
    options.AddPolicy(ConstantConfiguration.accessAllowed, policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<NurseLinkDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(ConstantConfiguration.defaultConnection)));

var jwtKey = builder.Configuration[ConstantConfiguration.jwtKey]!;
var jwtIssuer = builder.Configuration[ConstantConfiguration.jwtIssuer]!;
var jwtAudience = builder.Configuration[ConstantConfiguration.jwtAudience]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "NurseLink API v1");
        options.EnablePersistAuthorization();
    });
}

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseCors(ConstantConfiguration.accessAllowed);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();