using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OntoCms.Cli.Bootstrap;
using OntoCms.Cli.Smoke;

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

if (args.Length > 0 && args[0].Equals("smoke:post-save", StringComparison.OrdinalIgnoreCase))
{
    await PostSaveSmoke.RunAsync(configuration, logger);
    return 0;
}

if (args.Length > 0 && args[0].Equals("smoke:post-save-rollback", StringComparison.OrdinalIgnoreCase))
{
    await PostSaveSmoke.RunRollbackAsync(configuration, logger);
    return 0;
}

if (args.Length > 0 && args[0].Equals("smoke:post-bytag", StringComparison.OrdinalIgnoreCase))
{
    await PostSaveSmoke.RunByTagAsync(configuration, logger);
    return 0;
}

if (args.Length > 0 && args[0].Equals("smoke:post-tagids", StringComparison.OrdinalIgnoreCase))
{
    await PostSaveSmoke.RunTagIdsAsync(configuration, logger);
    return 0;
}

if (args.Length > 0 && args[0].Equals("smoke:menu-save", StringComparison.OrdinalIgnoreCase))
{
    await MenuSaveSmoke.RunAsync(configuration, logger);
    return 0;
}

if (args.Length > 0 && args[0].Equals("smoke:adv-save", StringComparison.OrdinalIgnoreCase))
{
    await AdvSaveSmoke.RunAsync(configuration, logger);
    return 0;
}

logger.LogError("Unknown CLI command. Supported commands: db:bootstrap, smoke:post-save, smoke:post-save-rollback, smoke:post-bytag, smoke:post-tagids, smoke:menu-save, smoke:adv-save");
return 1;
