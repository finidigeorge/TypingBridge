using Microsoft.Extensions.Configuration;
using TypingHandler;


var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();

var section = config.GetSection("AppConfig");

var handler = new Handler(section.Get<TypingBridgeConfig>());

handler.ReadSourceType();
await handler.DoType();

