using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OntoCms.Cli.Bootstrap;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();

using var host = builder.Build();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("OntoCms.Cli");
var configuration = host.Services.GetRequiredService<IConfiguration>();

if (args.Length > 0 && args[0].Equals("db:bootstrap", StringComparison.OrdinalIgnoreCase))
{
    await DatabaseBootstrapper.InitializeAsync(configuration, logger);
    return 0;
}

logger.LogError("Unknown CLI command. Supported commands: db:bootstrap");
return 1;