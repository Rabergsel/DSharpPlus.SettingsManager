using DSharpPlus.Entities;

namespace DSharpPlus.SettingsManager;


public class Manager
{
    internal List<SettingEntity<object>> defaults { get; set; } = new List<SettingEntity<object>>();
    
    private Dictionary<ulong, IReadOnlyList<SettingEntity<object>>> _settings { get; set; } = new();

    public IReadOnlyDictionary<ulong, IReadOnlyList<SettingEntity<object>>> Settings => _settings;

    public void Register(ulong id)
    {
        _settings.TryAdd(id, defaults.ToArray().ToList());
    }

    public void AddDefaultSetting(SettingEntity<object> entity)
    {
        defaults.Add(entity);
    }

    public bool HasID(ulong id)
    {
        return _settings.ContainsKey(id);
    }
    public bool HasName(string Name)
    {
        foreach(var d in defaults)
        {
            if(d.Name == Name) return true;
        }
        return false;
    }

    public dynamic GetSettingValue(ulong ID, string SettingName)
    {
        //Basic Checks to make our lives easier in the later code
        if(!HasID(ID)) { throw new Exception("Access to a non-existent ID was executed: " + ID); }
        if(!HasName(SettingName)) { throw new Exception("Access to a non-existent Setting was executed: " + SettingName); }

        return _settings[ID].First(s => (s.Name == SettingName || s.CommandAlts.Contains(SettingName))).Value;
    }

    public bool SetSettingValue(ulong ID, string SettingName, object Value)
    {
        //Basic Checks to make our lives easier in the later code
        if (!HasID(ID)) { throw new Exception("Access to a non-existent ID was executed: " + ID); }
        if (!HasName(SettingName)) { throw new Exception("Access to a non-existent Setting was executed: " + SettingName); }

        for(int i = 0; i < _settings[ID].Count; i++)
        {
            if (_settings[ID][i].Name==SettingName || _settings[ID][i].CommandAlts.Contains(SettingName))
            {
                _settings[ID][i].Value = Value;
                return true;
            }
        }
        return false;
    }

    public bool SetSettingValue(ulong ID, string SettingName, object Value, DiscordPermissions Permissions)
    {
        //Basic Checks to make our lives easier in the later code
        if (!HasID(ID)) { throw new Exception("Access to a non-existent ID was executed: " + ID); }
        if (!HasName(SettingName)) { throw new Exception("Access to a non-existent Setting was executed: " + SettingName); }

        for (int i = 0; i < _settings[ID].Count; i++)
        {
            if (_settings[ID][i].Name == SettingName || _settings[ID][i].CommandAlts.Contains(SettingName))
            {
                if (!PermissionMethods.HasPermission(Permissions, _settings[ID][i].Permissions)) return false;

                _settings[ID][i].Value = Value;
                return true;
            }
        }
        return false;
    }



}