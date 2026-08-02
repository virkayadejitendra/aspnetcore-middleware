using PartnerDataSharing.Api.Middleware;
using PartnerDataSharing.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<DemoDataStore>();
builder.Services.AddSingleton<AuditEventStore>();
builder.Services.AddScoped<RequestContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();

app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
app.UseMiddleware<DataSharingAuditMiddleware>();
app.UseMiddleware<PartnerAccessMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
