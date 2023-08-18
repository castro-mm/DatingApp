using System.Text;
using API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddApplicatonServices(builder.Configuration);

builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

// These two middlewares has to be declared AFTER UserCors' Middlwware, and BEFORE the MapControllers' middleware
app.UseAuthentication(); // Asks if you have a valid token.
app.UseAuthorization(); // Checks what you are allowed to do.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
