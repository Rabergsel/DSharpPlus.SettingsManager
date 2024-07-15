using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.SettingsManager;

public class SettingsManager : BaseExtension
{

    DiscordClient client;

    /// <summary>
    /// When set to true, the extension will set up a text command listener
    /// </summary>
    public bool CommandListener = true;

    /// <summary>
    /// The command listener will listen to all the messages with this prefix
    /// Only relevant when CommandListener is set to true
    /// </summary>
    public string prefix = "?";

    /// <summary>
    /// When set to true, the Manager will save after every change
    /// </summary>
    public bool SaveAfterEveryChange = true;

    /// <summary>
    /// The base folder for saving
    /// </summary>
    public string folder = "./settings/";

    /// <summary>
    /// The manager for Guild Settings
    /// </summary>
    Manager GuildSettings { get; set; } = new Manager();

    /// <summary>
    /// The manager for Channel Settings
    /// </summary>
    Manager ChannelSettings { get; set; } = new Manager();

    /// <summary>
    /// JSON Serializer for saving
    /// </summary>
    /// <param name="alternativeFolder">If left empty, the defined base folder will be used</param>
    public void SaveToJSON(string alternativeFolder = "")
    {
        string f = folder;
        if (alternativeFolder == "") f = alternativeFolder;

        if (!Directory.Exists(f))
        {
            Directory.CreateDirectory(f);
        }

        File.WriteAllText(f + "guildsettings.json", System.Text.Json.JsonSerializer.Serialize(GuildSettings));
        File.WriteAllText(f + "channelsettings.json", System.Text.Json.JsonSerializer.Serialize(ChannelSettings));

        if (client == null) return;
        client.Logger.Log(LogLevel.Information, new EventId(210, "Saving"), $"Finished saving of Managers into {f}");

    }

    /// <summary>
    /// Loads from JSON
    /// </summary>
    /// <param name="folderPath">Load from this folder</param>
    public void LoadFromJSON(string folderPath = "")
    {
        try
        {
            GuildSettings = System.Text.Json.JsonSerializer.Deserialize<Manager>(File.ReadAllText(folderPath   + "guildsettings.json"));
            ChannelSettings = System.Text.Json.JsonSerializer.Deserialize<Manager>(File.ReadAllText(folderPath + "channelsettings.json"));
        }
        catch (FileNotFoundException fnfex)
        {
            if(client == null) return;  
            client.Logger.Log(LogLevel.Error, new EventId(211, "Saving"), $"Couldn't load from {fnfex.FileName} as this file does not exist");
            
        }
    }

    /// <summary>
    /// A virtual public method for saving.
    /// If not overridden, it will use JSON Serializer
    /// </summary>
    public virtual void Save()
    {
        SaveToJSON();
    }

    /// <summary>
    /// A virtual public method for loading.
    /// If not overriden, it will load from the base folder with JSON
    /// </summary>
    public virtual void Load()
    {
        LoadFromJSON(folder);
    }


    /// <summary>
    /// Adds a default setting for a channel.
    /// This will be default for all new channel settings
    /// </summary>
    /// <param name="setting">The new default SettingEntity</param>
    public void AddDefaultChannelSetting(SettingEntity setting)
    {
        ChannelSettings.AddDefaultSetting(setting);
        if (SaveAfterEveryChange)
        {
            SaveToJSON();
        }
    }

    /// <summary>
    /// Adds a default setting for a guild.
    /// This will be default for all new guild settings
    /// </summary>
    /// <param name="setting">The new default SettingEntity</param>
    public void AddDefaultGuildSetting(SettingEntity setting)
    {
        GuildSettings.AddDefaultSetting(setting);
        if (SaveAfterEveryChange)
        {
            SaveToJSON();
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
        client.Logger.Log(LogLevel.Debug, new EventId(203, "Access"), $"Accessing setting of entity ID {id} with name {name}");
        string result = GuildSettings.GetSetting(id, name);
        if (result != null)
        {
            return result;
        }

        return ChannelSettings.GetSetting(id, name);
    }

    public bool SetSettingValue(ulong id, string name, string value)
    {
        client.Logger.Log(LogLevel.Debug, new EventId(204, "Access"), $"Changing setting of entity ID {id} with name {name} to {value}");
        if (GuildSettings.SetSetting(id, name, value))
        {
            return true;
        }

        if (ChannelSettings.SetSetting(id, name, value))
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

                CommandListenerFunction(e.Guild.Id, e.Channel.Id, PermissionMethods.HasPermission(e.Guild.GetMemberAsync(e.Author.Id).Result.Permissions, DiscordPermissions.Administrator), e.Message.Content, e.Channel);
            };
        }
    }

    private void RegisterAllGuilds(IReadOnlyDictionary<ulong, DiscordGuild> guilds)
    {

        foreach (KeyValuePair<ulong, DiscordGuild> guild in guilds)
        {
            client.Logger.Log(LogLevel.Debug, new EventId(202, "Register"), $"Registering Guild {guild.Value.Name} with ID {guild.Key}");
            GuildSettings.Register(guild.Key);

            foreach (KeyValuePair<ulong, DiscordChannel> channel in guild.Value.Channels)
            {
                client.Logger.Log(LogLevel.Debug, new EventId(203, "Register"), $"\tRegistering Channel {channel.Value.Name} with ID {channel.Key}");
                ChannelSettings.Register(channel.Key);
            }

        }
        client.Logger.Log(LogLevel.Information, new EventId(201, "Register"), $"Finished registering channels and guilds");

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


        client.Logger.Log(LogLevel.Debug, new EventId(204, "Access"), $"User tried changing Setting {name} to {value}; Is Admin? {isAdmin}");
        

        bool GuildSettingsSuccessfull = GuildSettings.SetSettingAsUser(guildId, name, value, isAdmin);
        bool ChannelSettingsSuccessful = ChannelSettings.SetSettingAsUser(channelId, name, value, isAdmin);

        if (GuildSettingsSuccessfull | ChannelSettingsSuccessful)
        {
            answer = $"Set Setting \"{name}\" to {value}";
            channel.SendMessageAsync(answer);
            if (SaveAfterEveryChange)
            {
                SaveToJSON();
            }

            return;
        }
    }


}