using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.SettingsManager;

public class SettingsManager : BaseExtension
{
    private DiscordClient client;

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
    private Manager GuildSettings { get; set; } = new Manager();

    /// <summary>
    /// The manager for Channel Settings
    /// </summary>
    private Manager ChannelSettings { get; set; } = new Manager();

    /// <summary>
    /// JSON Serializer for saving
    /// </summary>
    /// <param name="alternativeFolder">If left empty, the defined base folder will be used</param>
    public void SaveToJSON(string alternativeFolder = "")
    {
        string f = folder;
        if (alternativeFolder != "")
        {
            f = alternativeFolder;
        }

        if (!Directory.Exists(f))
        {
            Directory.CreateDirectory(f);
        }

        File.WriteAllText(f + "guildsettings.json", System.Text.Json.JsonSerializer.Serialize(GuildSettings));
        File.WriteAllText(f + "channelsettings.json", System.Text.Json.JsonSerializer.Serialize(ChannelSettings));

        if (client == null)
        {
            return;
        }

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
            GuildSettings = System.Text.Json.JsonSerializer.Deserialize<Manager>(File.ReadAllText(folderPath + "guildsettings.json"));
            ChannelSettings = System.Text.Json.JsonSerializer.Deserialize<Manager>(File.ReadAllText(folderPath + "channelsettings.json"));
        }
        catch (FileNotFoundException fnfex)
        {
            if (client == null)
            {
                return;
            }

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
    public void AddDefaultChannelSetting(SettingEntity<object> setting)
    {
        ChannelSettings.AddDefaultSetting(setting);
        if (SaveAfterEveryChange)
        {
            Save();
        }
    }

    /// <summary>
    /// Adds a default setting for a guild.
    /// This will be default for all new guild settings
    /// </summary>
    /// <param name="setting">The new default SettingEntity</param>
    public void AddDefaultGuildSetting(SettingEntity<object> setting)
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

    public dynamic GetSettingValue(ulong id, string name)
    {
        client.Logger.Log(LogLevel.Debug, new EventId(203, "Access"), $"Accessing setting of entity ID {id} with name {name}");
        string result = GuildSettings.GetSettingValue(id, name);
        if (result != null)
        {
            return result;
        }

        return ChannelSettings.GetSettingValue(id, name);
    }

    public bool SetSettingValue(ulong id, string name, object value)
    {
        client.Logger.Log(LogLevel.Debug, new EventId(204, "Access"), $"Changing setting of entity ID {id} with name {name} to {value}");
        if (ChannelSettings.HasID(id))
        {
            ChannelSettings.SetSettingValue(id, name, value);
        }

        if (GuildSettings.HasID(id))
        {
            GuildSettings.SetSettingValue(id, name, value);
        }

        return false;
    }

    public override void Dispose()
    {

    }

    public bool Registered = false;
    public void Register(ref DiscordClientBuilder builder)
    {
        builder.ConfigureEventHandlers
         (
             b => b.HandleGuildDownloadCompleted(async (s, e) =>
             {
                 RegisterAllGuilds(e.Guilds);
             })
         );


        if (CommandListener)
        {
            builder.ConfigureEventHandlers
          (
              b => b.HandleMessageCreated(async (s, e) =>
              {
                  if (e.Guild is null)
                  {
                      return;
                  }

                  CommandListenerFunction(e.Guild.Id, e.Channel.Id, e.Channel.PermissionsFor(await e.Guild.GetMemberAsync(e.Author.Id)), e.Message.Content, e.Channel);
              })
          );
        }
        Registered = true;
    }

    protected override void Setup(DiscordClient client)
    {

        if (!Registered)
        {
            throw new Exception("Call SettingsManager.Register() before building the discord client!");
        }

        this.client = client;
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

    private void CommandListenerFunction(ulong guildId, ulong channelId, DiscordPermissions Permissions, string content, DiscordChannel channel)
    {

        if (!content.StartsWith(prefix))
        {
            return;
        }

        string answer = "";
        if (content.StartsWith(prefix + "help"))
        {
            foreach (var d in GuildSettings.defaults)
            {
                if (!PermissionMethods.HasPermission(Permissions, d.Permissions))
                {
                    continue;
                }

                answer += "**" + prefix + d.Name + "**\t*" + d.Description + "*\n";
                if (d.AllowedValues.Count() != 0)
                {
                    answer += "Allowed: ";
                    foreach (var v in d.AllowedValues)
                    {
                        answer += v.ToString() + "|";
                    }
                }

            }
            foreach (var d in ChannelSettings.defaults)
            {
                if (!PermissionMethods.HasPermission(Permissions, d.Permissions))
                {
                    continue;
                }

                answer += "**" + d.Name + "**\t*" + d.Description + "*\n";

            }
            channel.SendMessageAsync(answer);
            return;
        }


        content = content.Replace(prefix, "").Trim();

        string name = content.Split(" ")[0].Trim();
        string value = content.Replace(name, "").Trim();


        client.Logger.Log(LogLevel.Debug, new EventId(204, "Access"), $"User tried changing Setting {name} to {value}");

        bool GuildSettingsSuccessfull = false;
        bool ChannelSettingsSuccessfull = false;
        try
        {
            if (GuildSettings.HasID(guildId))
            {
                GuildSettingsSuccessfull = GuildSettings.SetSettingValue(guildId, name, value, Permissions);
            }
            if (ChannelSettings.HasID(guildId))
            {
                ChannelSettingsSuccessfull = ChannelSettings.SetSettingValue(guildId, name, value, Permissions);
            }
        }
        catch (Exception ex)
        {
            answer = ex.Message + "\nUse **" + prefix + "help** to see all commands and their info";
            channel.SendMessageAsync(answer);
        }

        if (GuildSettingsSuccessfull | ChannelSettingsSuccessfull)
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