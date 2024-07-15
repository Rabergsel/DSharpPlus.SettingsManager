namespace DSharpPlus.SettingsManager;


public class Manager
{
    internal List<SettingEntity> defaults = new List<SettingEntity>();
    
    private readonly Dictionary<ulong, IReadOnlyList<SettingEntity>> _settings = new();

    public IReadOnlyDictionary<ulong, IReadOnlyList<SettingEntity>> Settings => _settings;

    public void Register(ulong id)
    {
        _settings.TryAdd(id, defaults.ToArray().ToList());
    }

    public void AddDefaultSetting(SettingEntity newSetting)
    {
        foreach (SettingEntity? def in defaults)
        {
            if (def.Name == newSetting.Name)
            {
                return; //Name already in use
            }
        }

        defaults.Add(newSetting);

        foreach (KeyValuePair<ulong, IReadOnlyList<SettingEntity>> setting in _settings)
        {
            ((List<SettingEntity>)setting.Value).Add(newSetting);
        }

    }

    public bool SetSettingAsUser(ulong id, string name, string value, bool isAdmin)
    {
        if (Settings.ContainsKey(id))
        {
            for (int i = 0; i < Settings[id].Count; i++)
            {
                if (Settings[id][i].Name == name)
                {
                    if (Settings[id][i].needsAdmin & !isAdmin) { return false; } //Missing Privileges

                    Settings[id][i].Value = value;
                    return true;
                }

                if (Settings[id][i].CommandAlts.Contains(name))
                {
                    if (Settings[id][i].needsAdmin & !isAdmin) { return false; } //Missing Privileges

                    Settings[id][i].Value = value;
                    return true;
                }

            }
        }

        return false;
    }

    public bool SetSetting(ulong id, string name, string value)
    {
        if (Settings.ContainsKey(id))
        {
            for (int i = 0; i < Settings.Count; i++)
            {
                if (Settings[id][i].Name == name)
                {
                    Settings[id][i].Value = value;
                    return true;
                }

                if (Settings[id][i].CommandAlts.Contains(name))
                {
                    Settings[id][i].Value = value;
                    return true;
                }

            }
        }

        return false;
    }

    public string GetSetting(ulong id, string name, bool registerNew = false)
    {
        if (Settings.ContainsKey(id))
        {
            foreach (SettingEntity? Setting in Settings[id])
            {
                if (Setting.Name == name)
                {
                    return Setting.Value;
                }

                if (Setting.CommandAlts.Contains(name))
                {
                    return Setting.Value;
                }
            }
        }
        else
        {
            if (registerNew)
            {
                _settings.Add(id, defaults.ToArray().ToList()); //Converting it so it is cloned
                return GetSetting(id, name);
            }
        }

        return null;
    }


}