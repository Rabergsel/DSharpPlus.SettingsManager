using DSharpPlus;
using DSharpPlus.Entities;

namespace DSharpPlus.SettingsManager
{
    public class SettingsManager : BaseExtension
    {

        DiscordClient client;

        //0 is off
        //1 is only errors and most important events
        //2 is Actions
        //3 is Looping Debugging
        public int DebugLevel = 0;

        public bool CommandListener = true;
        public string prefix = "?";


        Manager.Manager GuildSettings { get; set; } = new Manager.Manager();
        Manager.Manager ChannelSettings { get; set; } = new Manager.Manager();

        private void log(string prefix, string log, int level)
        {
            if(level <= DebugLevel)
            {
                Console.WriteLine($"{DateTime.Now}\t[{prefix.ToUpper()}] {log}");
            }
        }
        
       

        public string GetSettingValue(ulong id, string name)
        {
            log("SM", "Trying to get Setting " + name + " for " + id, 2);
            string result = GuildSettings.getSetting(id, name);
            if(result != null) return result;

            return ChannelSettings.getSetting(id, name);
        }

        public bool SetSettingValue(ulong id, string name, string value)
        {
            log("SM", "Trying to set Setting " + name + " for " + id + " to " + value, 2);
            if (GuildSettings.setSetting(id, name, value))
            {
                return true;
            }

            if(ChannelSettings.setSetting(id, name, value))
            {
                return true;
            }

            return false;
        }

        public override void Dispose()
        {
            
        }

        protected override void Setup(DiscordClient client)
        {
            this.client = client;

            client.GuildDownloadCompleted += async (s, e) =>
            {
                RegisterAllGuilds(e.Guilds);
            };

            if(CommandListener)
            {
                client.MessageCreated += async (s, e) =>
                {
                    if (e.Guild == null) return;
                    CommandListenerFunction(e.Guild.Id, e.Channel.Id, PermissionMethods.HasPermission(e.Guild.GetMemberAsync(e.Author.Id).Result.Permissions, Permissions.Administrator), e.Message.Content, e.Channel);
                };
            }
        }

        private void RegisterAllGuilds(IReadOnlyDictionary<ulong, DiscordGuild> guilds)
        {
            
            foreach(var guild in guilds)
            {
                log("SM", $"Registering Guild \"{guild.Value.Name}\"(ID: {guild.Key}) into Registry", 3);
                GuildSettings.Register(guild.Key);

                foreach(var channel in guild.Value.Channels)
                {
                    log("SM", $"Registering Channel \"{channel.Value.Name}\"(ID: {channel.Key}) into Registry", 3);
                    ChannelSettings.Register(channel.Key);
                }

            }
            log("SM", "Finished registering all Guilds and Channels into Settings Registry", 1);

        }

        private void CommandListenerFunction(ulong guildId, ulong channelId, bool isAdmin, string content, DiscordChannel channel)
        {

            if (!content.StartsWith(prefix)) return;

            string answer = "";
            if(content.StartsWith(prefix + " help"))
            {
                foreach(var d in GuildSettings.defaults)
                {
                    if(!isAdmin & d.needsAdmin)
                    {
                        continue;
                    }

                    answer += "**" + d.Name + "**\t" + d.Description + "\n";

                }
                foreach (var d in ChannelSettings.defaults)
                {
                    if (!isAdmin & d.needsAdmin)
                    {
                        continue;
                    }

                    answer += "**" + d.Name + "**\t" + d.Description + "\n";

                }
                channel.SendMessageAsync(answer);
                return;
            }


            content.Replace(prefix, "");

            string name = content.Split(" ")[0].Trim();
            string value = content.Replace(name, "").Trim();

            GuildSettings.setSettingAsUser(guildId, name, value, isAdmin);
            ChannelSettings.setSettingAsUser(channelId, name, value, isAdmin);

        }


    }
}