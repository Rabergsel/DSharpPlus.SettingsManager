# Rabergsel.DSharpPlus.SettingsManager

Rabergsel.DSharpPlus.SettingsManager is an extension for DSharpPlus that provides an easy infrastructure for managing settings for Discord Servers and Channels.

## Installation

This package is under heavy development and not yet available on NuGet. However, you can clone the repository and build the project locally.

## Usage

With DSharpPlus 5.0 (nightly), the process of building a discord client has changed.
This has been patched with SettingsManager v1.2.0, so if you are using an older version of DSharpPlus, please use an older version of this package.

This example is a minimum example that makes use of all important features of the library.

```csharp
using DSharpPlus;
using DSharpPlus.SettingsManager;
using Microsoft.Extensions.Logging;

namespace MyFirstBot
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            //Create Discord Builder
            DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(File.ReadAllText("token.txt"), DiscordIntents.All);

            SettingsManager? settings = new SettingsManager();
            settings.Register(ref builder);   //!!! ALWAYS CALL BEFORE REGISTERING

            //The following section registers an event that will respond "pong" to "ping" as often
            //as set in the "pings" setting.
            builder.ConfigureEventHandlers
                (
                    b => b.HandleMessageCreated(async (s, e) =>
                    {
                        if (e.Message.Content.ToLower().StartsWith("ping"))
                        {
                            for (int i = 0; i < settings.GetSettingValueAsLong(e.Guild.Id, "pings"); i++)
                            {
                                await e.Message.RespondAsync("pong " + i);
                            }
                        }
                    })
                );

            var discord = builder.Build();

            //Add Extension
            discord.AddExtension(settings);

            settings.AddDefaultGuildSetting(new SettingEntity<object>("pings", 1)
            {
                AllowedValues = new List<object>() { 0, 1, 3, 5 }    //Only allow the values of 0, 1, 3 and 5
            });

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
```


## Note

This package is under heavy development and may undergo frequent changes. It's recommended to keep an eye on updates and consult the documentation for any breaking changes.

## TODO
- User settings
- Complete generic User Settings
- More customization
