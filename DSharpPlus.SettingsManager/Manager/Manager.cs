namespace DSharpPlus.SettingsManager.Manager
{
    public class Manager
    {
        internal List<SettingEntity> defaults = new List<SettingEntity>();

        public Dictionary<ulong, List<SettingEntity>> Settings { get; set; } = new Dictionary<ulong, List<SettingEntity>>();

        /// <summary>
        /// Register object
        /// </summary>
        /// <param name="id">ID</param>
        public void Register(ulong id)
        {
            if (Settings.ContainsKey(id))
            {
                return;
            }
            else
            {
                Settings.Add(id, defaults.ToArray().ToList());
            }
        }

        /// <summary>
        /// Adds a setting to all objects
        /// </summary>
        /// <param name="setting">New setting</param>
        public void AddDefaultSetting(SettingEntity setting)
        {

            foreach (SettingEntity? def in defaults)
            {
                if (def.Name == setting.Name)
                {
                    return; //Name already in use
                }
            }

            defaults.Add(setting);

            foreach (KeyValuePair<ulong, List<SettingEntity>> Setting in Settings)
            {
                Setting.Value.Add(setting);
            }

        }

        /// <summary>
        /// Sets a setting, but also checks admin privileges
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="name">Setting name</param>
        /// <param name="value">New Value</param>
        /// <param name="isAdmin">Is Admin?</param>
        /// <returns>Success</returns>
        public bool setSettingAsUser(ulong id, string name, string value, bool isAdmin)
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

        /// <summary>
        /// Sets setting regardless of permission
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="name">Name</param>
        /// <param name="value">New Value</param>
        /// <returns>Success</returns>
        public bool setSetting(ulong id, string name, string value)
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

        /// <summary>
        /// Gets Setting
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="name">Name of setting</param>
        /// <param name="registerNew">Boolean wheter a new object should be created if the ID is not found in the dictionary, by default is false</param>
        /// <returns>Settings value</returns>
        public string getSetting(ulong id, string name, bool registerNew = false)
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
                    Settings.Add(id, defaults.ToArray().ToList()); //Converting it so it is cloned
                    return getSetting(id, name);
                }
            }

            return null;
        }


    }
}
