using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.SettingsManager
{
    public class SettingsManager : BaseExtension
    {

        DiscordClient client;


        /// <summary>
        /// Debug Level
        /// 0 is off
        /// 1 is only errors and most important events
        /// 2 is Actions
        /// 3 is Looping Debugging
        /// </summary>
        public int DebugLevel = 0;

        /// <summary>
        /// Wheter to listen to commands
        /// </summary>
        public bool CommandListener = true;

        /// <summary>
        /// Prefix for setting change command
        /// </summary>
        public string prefix = "?";

        /// <summary>
        /// Should settings file be saved after every change
        /// </summary>
        public bool SaveAfterEveryChange = true;

        /// <summary>
        /// Saving folder
        /// </summary>
        public string folder = "./settings/";


        /// <summary>
        /// Manager instance for Guilds
        /// </summary>
        Manager.Manager GuildSettings { get; set; } = new Manager.Manager();

        /// <summary>
        /// Manager instance for Channels
        /// </summary>
        Manager.Manager ChannelSettings { get; set; } = new Manager.Manager();

        /// <summary>
        /// Returns Serialization String of this Manager
        /// </summary>
        /// <returns>JSON String</returns>
        public string Serialize()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }

        /// <summary>
        /// Saves this object to folder provided
        /// </summary>
        public void Save()
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(folder + "guildsettings.json", System.Text.Json.JsonSerializer.Serialize(GuildSettings));
            File.WriteAllText(folder + "channelsettings.json", System.Text.Json.JsonSerializer.Serialize(ChannelSettings));

            if (client == null) return;
            client.Logger.Log(LogLevel.Information, new EventId(210, "Saving"), "Setting Files have been written to provided folder path: " + folder);
            
        }

        /// <summary>
        /// Load from folder provided
        /// </summary>
        /// <param name="folderPath">Optional folder overwrite</param>
        public void Load(string folderPath = "")
        {
            try
            {
                GuildSettings = System.Text.Json.JsonSerializer.Deserialize<Manager.Manager>(File.ReadAllText(folderPath + "guildsettings.json"));
                ChannelSettings = System.Text.Json.JsonSerializer.Deserialize<Manager.Manager>(File.ReadAllText(folderPath + "channelsettings.json"));
            }
            catch (FileNotFoundException fnfex)
            {
                if (client == null) return;
                client.Logger.Log(LogLevel.Error, new EventId(220, "Loading"), "Couldn't load from " + fnfex.FileName + " as this file does not exist");
            }
        }

        /// <summary>
        /// Adds a setting to Channels.
        /// </summary>
        /// <param name="setting">The default setting value</param>
        public void AddDefaultChannelSetting(SettingEntity setting)
        {
            ChannelSettings.AddDefaultSetting(setting);
            if (SaveAfterEveryChange)
            {
                Save();
            }
        }
        /// <summary>
        /// Adds a setting to Guilds.
        /// </summary>
        /// <param name="setting">The default setting value</param>
        public void AddDefaultGuildSetting(SettingEntity setting)
        {
            GuildSettings.AddDefaultSetting(setting);
            if (SaveAfterEveryChange)
            {
                Save();
            }
        }

        /// <summary>
        /// Gets setting value as long
        /// </summary>
        /// <param name="id">Id of Channel or Guild</param>
        /// <param name="name">Name of setting</param>
        /// <param name="defaultValue">If no setting is found, this value will be returned</param>
        /// <returns>Setting value as long</returns>
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

        /// <summary>
        /// Gets setting value as float
        /// </summary>
        /// <param name="id">Id of Channel or Guild</param>
        /// <param name="name">Name of setting</param>
        /// <param name="defaultValue">If no setting is found, this value will be returned</param>
        /// <returns>Setting value as float</returns>
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
        /// <summary>
        /// Gets setting value as Boolean
        /// </summary>
        /// <param name="id">Id of Channel or Guild</param>
        /// <param name="name">Name of setting</param>
        /// <param name="defaultValue">If no setting is found, this value will be returned</param>
        /// <returns>Setting value as bool</returns>
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

        /// <summary>
        /// Gets setting value as string
        /// </summary>
        /// <param name="id">Id of Channel or Guild</param>
        /// <param name="name">Name of setting</param>
        /// <returns>Setting value as string</returns>
        public string GetSettingValue(ulong id, string name)
        {
            client.Logger.Log(LogLevel.Debug, new EventId(201, "Access"), $"Trying to get Setting {name} for {id}");
            string result = GuildSettings.getSetting(id, name);
            if (result != null)
            {
                return result;
            }

            return ChannelSettings.getSetting(id, name);
        }

        /// <summary>
        /// Sets setting value
        /// </summary>
        /// <param name="id">Id of channel or guild.</param>
        /// <param name="name">Setting name</param>
        /// <param name="value">New value of setting</param>
        /// <returns>Success</returns>
        public bool SetSettingValue(ulong id, string name, string value)
        {
            client.Logger.Log(LogLevel.Debug, new EventId(201, "Access"), $"Trying to set Setting {name} for {id} to {value}");
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

        /// <summary>
        /// Function to manage Disposal
        /// </summary>
        public override void Dispose()
        {

        }

        /// <summary>
        /// Function to setup Manager
        /// </summary>
        /// <param name="client">DiscordClient to register to</param>
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

        /// <summary>
        /// Private Function to register all guilds and channels
        /// </summary>
        /// <param name="guilds">Guilds Dictionary</param>
        private void RegisterAllGuilds(IReadOnlyDictionary<ulong, DiscordGuild> guilds)
        {

            foreach (KeyValuePair<ulong, DiscordGuild> guild in guilds)
            {
                client.Logger.Log(LogLevel.Trace, new EventId(202, "Startup"), $"Registering Guild \"{guild.Value.Name}\"(ID: {guild.Key}) into Registry");


                GuildSettings.Register(guild.Key);

                foreach (KeyValuePair<ulong, DiscordChannel> channel in guild.Value.Channels)
                {
                    client.Logger.Log(LogLevel.Trace, new EventId(202, "Startup"), $"Registering Channel \"{channel.Value.Name}\"(ID: {channel.Key}) into Registry");
                    ChannelSettings.Register(channel.Key);
                }

            }

            client.Logger.Log(LogLevel.Information, new EventId(202, "Startup"), "Finished registering all Guilds and Channels into Settings Registry");

        }

        /// <summary>
        /// Function for Command Listener
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="channelId">Channel ID</param>
        /// <param name="isAdmin">Whether user is Admin or not</param>
        /// <param name="content">Content of message </param>
        /// <param name="channel">DiscordChannel Object</param>
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

            client.Logger.Log(LogLevel.Debug, new EventId(2), $"User trying to set setting {name} to {value}; Is Admin? {isAdmin}");

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