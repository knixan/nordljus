// Backend för lägesbilden. Ingest, händelselogg och SignalR-hubb läggs till i V2.
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run();
