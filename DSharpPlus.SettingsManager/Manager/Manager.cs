using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.SettingsManager.Manager
{
    public class Manager
    {
        internal List<SettingEntity> defaults = new List<SettingEntity>();

        Dictionary<ulong, List<SettingEntity>> Settings { get; set; } = new Dictionary<ulong, List<SettingEntity>>();

        public void Register(ulong id)
        {
            if (Settings.ContainsKey(id)) return;
            else Settings.Add(id, defaults.ToArray().ToList());
        }

        public void AddDefaultSetting(SettingEntity setting)
        {

            foreach(var def in defaults)
            {
                if(def.Name == setting.Name)
                {
                    return; //Name already in use
                }
            }

            defaults.Add(setting);

            foreach(var Setting in Settings)
            {
                Setting.Value.Add(setting);
            }

        }

        public bool setSettingAsUser(ulong id, string name, string value, bool isAdmin)
        {
            if (Settings.ContainsKey(id))
            {
                for (int i = 0; i < Settings[id].Count; i++)
                {
                    if (Settings[id][i].Name == name)
                    {
                        if(Settings[id][i].needsAdmin & !isAdmin) { return false; } //Missing Privileges

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

        public bool setSetting(ulong id, string name, string value)
        {
            if (Settings.ContainsKey(id))
            {
                for(int i = 0; i < Settings.Count; i++)
                {
                    if(Settings[id][i].Name == name)
                    {
                        Settings[id][i].Value = value;
                        return true;
                    }

                    if(Settings[id][i].CommandAlts.Contains(name))
                    {
                        Settings[id][i].Value = value;
                        return true;
                    }

                }
            }

            return false;
        }

        public string getSetting(ulong id, string name, bool registerNew = false)
        {
            if(Settings.ContainsKey(id))
            {
                foreach(var Setting in Settings[id])
                {
                    if(Setting.Name == name) return Setting.Value;
                    if(Setting.CommandAlts.Contains(name)) return Setting.Value;

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
