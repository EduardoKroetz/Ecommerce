using System.Text.Json.Serialization;
using Ecommerce.Api.Middlewares;
using Ecommerce.Application.Extensions;
using Ecommerce.Infra.Data;
using Ecommerce.Infra.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

[assembly: ApiController]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, ctx, ct) =>
    {
        doc.Components ??= new();
        doc.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        doc.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme()
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };
        return Task.CompletedTask;
    });
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
dbContext.Database.Migrate();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapOpenApi();
app.MapScalarApiReference(x => x
    .AddPreferredSecuritySchemes(["Bearer"])
    .EnablePersistentAuthentication());

app.Run();
