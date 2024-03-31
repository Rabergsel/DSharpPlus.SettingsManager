# Rabergsel.DSharpPlus.SettingsManager

Rabergsel.DSharpPlus.SettingsManager is an extension for DSharpPlus that provides an easy infrastructure for managing settings for Discord Servers and Channels.

## Installation

This package is under heavy development and not yet available on NuGet. However, you can clone the repository and build the project locally.

## Usage

```csharp
using DSharpPlus;  // Import DSharpPlus library for Discord API
using DSharpPlus.SettingsManager;  // Import DSharpPlus.SettingsManager library for managing settings
using System.IO;  // Import System.IO for file reading

namespace MyFirstBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Discord client
            DiscordClient? discord = new DiscordClient(new DiscordConfiguration()
            {
                // Read bot token from token.txt file
                Token = File.ReadAllText("token.txt"),
                TokenType = TokenType.Bot,  // Set token type as Bot
                Intents = DiscordIntents.All  // Enable all intents for the bot
            });

            // Add default guild setting for reacting to "ping" message
            settings.AddDefaultGuildSetting(new SettingEntity("ReactToPing", false.ToString(), "If set to yes, the bot will react to a \"ping\" message", false));

            // Add SettingsManager extension to Discord client
            discord.AddExtension(settings);

            // Handle message creation event
            discord.MessageCreated += async (s, e) =>
            {
                // Check if guild setting allows reacting to "ping" message
                if (!settings.GetSettingValueAsBoolean(e.Guild.Id, "ReactToPing", false))
                {
                    return;  // Skip if not allowed
                }

                // Check if message content starts with "ping"
                if (e.Message.Content.ToLower().StartsWith("ping"))
                {
                    // Respond with "pong!"
                    await e.Message.RespondAsync("pong!");
                }
            };

            // Connect to Discord
            await discord.ConnectAsync();

            // Keep the application running indefinitely
            await Task.Delay(-1);
        }
    }
}
```


## Note

This package is under heavy development and may undergo frequent changes. It's recommended to keep an eye on updates and consult the documentation for any breaking changes.

User Settings are coming soon.
