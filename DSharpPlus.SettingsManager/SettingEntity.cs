using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.SettingsManager;

public class SettingEntity<T>
{

    public SettingEntity() { }
    public SettingEntity(string Name, T Value)
    {
        this.Name = Name;
        this.Value = Value;
    }

    public string Name { get; set; }
    public string Description { get; set; } = "";

    public T Value { get; set; }

    public IEnumerable<T> AllowedValues { get; set; } = new List<T>();
    public IEnumerable<string> CommandAlts { get; set; } = new List<string>();

    public DiscordPermissions Permissions { get; set; } = DiscordPermissions.None;

}