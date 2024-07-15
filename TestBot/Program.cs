using DSharpPlus;
using DSharpPlus.SettingsManager;
using Newtonsoft.Json.Linq;

namespace MyFirstBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Create Discord Builder
            DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(File.ReadAllText("token.txt"), DiscordIntents.All);
            var discord = builder.Build();

            //Add Extension
            SettingsManager? settings = new SettingsManager();
            discord.AddExtension(settings);
            

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}