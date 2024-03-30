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

        public bool SaveAfterEveryChange = true;
        public string folder = "./settings/";


        Manager.Manager GuildSettings { get; set; } = new Manager.Manager();
        Manager.Manager ChannelSettings { get; set; } = new Manager.Manager();

        private void log(string prefix, string log, int level)
        {
            if (level <= DebugLevel)
            {
                Console.WriteLine($"{DateTime.Now}\t[{prefix.ToUpper()}] {log}");
            }
        }


        public string Serialize()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }


        public void Save()
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(folder + "guildsettings.json", System.Text.Json.JsonSerializer.Serialize(GuildSettings));
            File.WriteAllText(folder + "channelsettings.json", System.Text.Json.JsonSerializer.Serialize(ChannelSettings));
            log("SM", "Saved Manager", 2);
        }

        public void Load(string folderPath = "")
        {
            try
            {
                GuildSettings = System.Text.Json.JsonSerializer.Deserialize<Manager.Manager>(File.ReadAllText(folderPath + "guildsettings.json"));
                ChannelSettings = System.Text.Json.JsonSerializer.Deserialize<Manager.Manager>(File.ReadAllText(folderPath + "channelsettings.json"));
            }
            catch (FileNotFoundException fnfex)
            {
                log("SM", "Couldn't load from " + fnfex.FileName + " as this file does not exist", 1);
            }
        }

        public void AddDefaultChannelSetting(SettingEntity setting)
        {
            ChannelSettings.AddDefaultSetting(setting);
            if (SaveAfterEveryChange)
            {
                Save();
            }
        }

        public void AddDefaultGuildSetting(SettingEntity setting)
        {
            GuildSettings.AddDefaultSetting(setting);
            if (SaveAfterEveryChange)
            {
                Save();
            }
        }

        public long GetSettingValueAsLong(ulong id, string name, long defaultValue = 0)
        {
            try
            {
                return long.Parse(GetSettingValue(id, name));
            }
            catch
            {
                return defaultValue;
            }
        }

        public float GetSettingValueAsFloat(ulong id, string name, float defaultValue = 0)
        {
            try
            {
                return float.Parse(GetSettingValue(id, name));
            }
            catch
            {
                return defaultValue;
            }
        }

        public bool GetSettingValueAsBoolean(ulong id, string name, bool defaultValue = false)
        {
            try
            {
                return bool.Parse(GetSettingValue(id, name));
            }
            catch
            {
                return defaultValue;
            }
        }

        public string GetSettingValue(ulong id, string name)
        {
            log("SM", "Trying to get Setting " + name + " for " + id, 2);
            string result = GuildSettings.getSetting(id, name);
            if (result != null)
            {
                return result;
            }

            return ChannelSettings.getSetting(id, name);
        }

        public bool SetSettingValue(ulong id, string name, string value)
        {
            log("SM", "Trying to set Setting " + name + " for " + id + " to " + value, 2);
            if (GuildSettings.setSetting(id, name, value))
            {
                return true;
            }

            if (ChannelSettings.setSetting(id, name, value))
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

            if (CommandListener)
            {
                client.MessageCreated += async (s, e) =>
                {
                    if (e.Guild == null)
                    {
                        return;
                    }

                    CommandListenerFunction(e.Guild.Id, e.Channel.Id, PermissionMethods.HasPermission(e.Guild.GetMemberAsync(e.Author.Id).Result.Permissions, Permissions.Administrator), e.Message.Content, e.Channel);
                };
            }
        }

        private void RegisterAllGuilds(IReadOnlyDictionary<ulong, DiscordGuild> guilds)
        {

            foreach (KeyValuePair<ulong, DiscordGuild> guild in guilds)
            {
                log("SM", $"Registering Guild \"{guild.Value.Name}\"(ID: {guild.Key}) into Registry", 3);
                GuildSettings.Register(guild.Key);

                foreach (KeyValuePair<ulong, DiscordChannel> channel in guild.Value.Channels)
                {
                    log("SM", $"Registering Channel \"{channel.Value.Name}\"(ID: {channel.Key}) into Registry", 3);
                    ChannelSettings.Register(channel.Key);
                }

            }
            log("SM", "Finished registering all Guilds and Channels into Settings Registry", 1);

        }

        private void CommandListenerFunction(ulong guildId, ulong channelId, bool isAdmin, string content, DiscordChannel channel)
        {

            if (!content.StartsWith(prefix))
            {
                return;
            }

            string answer = "";
            if (content.StartsWith(prefix + " help"))
            {
                foreach (SettingEntity? d in GuildSettings.defaults)
                {
                    if (!isAdmin & d.needsAdmin)
                    {
                        continue;
                    }

                    answer += "**" + d.Name + "**\t" + d.Description + "\n";

                }
                foreach (SettingEntity? d in ChannelSettings.defaults)
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


            content = content.Replace(prefix, "").Trim();

            string name = content.Split(" ")[0].Trim();
            string value = content.Replace(name, "").Trim();

            log("SM", $"User trying to set setting {name} to {value}; Is Admin? {isAdmin}", 2);

            bool GuildSettingsSuccessfull = GuildSettings.setSettingAsUser(guildId, name, value, isAdmin);
            bool ChannelSettingsSuccessful = ChannelSettings.setSettingAsUser(channelId, name, value, isAdmin);

            if (GuildSettingsSuccessfull | ChannelSettingsSuccessful)
            {
                answer = $"Set Setting \"{name}\" to {value}";
                channel.SendMessageAsync(answer);
                if (SaveAfterEveryChange)
                {
                    Save();
                }

                return;
            }
        }


    }
}