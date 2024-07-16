using DSharpPlus;
using DSharpPlus.SettingsManager;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace MyFirstBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Create Discord Builder
            DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(File.ReadAllText("token.txt"), DiscordIntents.All);
            builder.SetLogLevel(LogLevel.Trace);

            SettingsManager? settings = new SettingsManager();
            settings.Register(ref builder);



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
                AllowedValues = new List<object>() {0, 1, 3, 5 }
            });

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}