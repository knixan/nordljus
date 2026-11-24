using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Nordljus.Backend.Ingest;
using Nordljus.Backend.Situation;
using Nordljus.Core;

// Backend för lägesbilden: tar emot sensordata (ingest) och skickar ut den i realtid (SignalR).
var builder = WebApplication.CreateBuilder(args);

// Fel svarar med ProblemDetails utan stacktrace.
builder.Services.AddProblemDetails();

// Enum som text ("groundVehicle") i stället för tal, så att frontend inte behöver känna till ordningen.
builder.Services.AddSignalR().AddJsonProtocol(o =>
    o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.AddOptions<UdpIngestOptions>()
    .Bind(builder.Configuration.GetSection(UdpIngestOptions.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<ValidationOptions>()
    .Bind(builder.Configuration.GetSection(ValidationOptions.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(sp => new TrackValidator(
    sp.GetRequiredService<TimeProvider>(),
    sp.GetRequiredService<IOptions<ValidationOptions>>().Value.MaxClockSkew));
builder.Services.AddHostedService<UdpIngestService>();

var app = builder.Build();

app.UseExceptionHandler();
app.MapHub<SituationHub>("/hubs/situation");

app.Run();
