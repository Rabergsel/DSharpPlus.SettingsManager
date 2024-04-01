using DSharpPlus;
using DSharpPlus.SettingsManager;

namespace MyFirstBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DiscordClient? discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = File.ReadAllText("token.txt"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            });

            SettingsManager? settings = new SettingsManager();


            discord.AddExtension(settings);

            discord.MessageCreated += async (s, e) =>
            {
                if (settings.GetSettingValueAsBoolean(e.Guild.Id, "ReactToPing", false))
                {
                    return;
                }

                if (e.Message.Content.ToLower().StartsWith("ping"))
                {
                    await e.Message.RespondAsync("pong!");
                }
            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}