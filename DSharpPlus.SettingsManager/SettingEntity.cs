namespace DSharpPlus.SettingsManager
{
    public class SettingEntity
    {
        /// <summary>
        /// Creates a SettingEntity object
        /// </summary>
        public SettingEntity() { }

        /// <summary>
        /// Creates a SettingEntity object
        /// </summary>
        /// <param name="Name">The name of the setting, must be unique amongst all settings</param>
        /// <param name="Value">The default value of the setting</param>
        public SettingEntity(string Name, string Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

        /// <summary>
        /// Creates a SettingEntity object
        /// </summary>
        /// <param name="name">The name of the setting, must be unique amongst all settings</param>
        /// <param name="value">The default value of the setting</param>
        /// <param name="description">The description shown when using the help command</param>
        /// <param name="needsAdmin">Set to true if only admins should be able to change it</param>
        public SettingEntity(string name, string value, string description, bool needsAdmin)
        {
            Name = name;
            Value = value;
            Description = description;
            this.needsAdmin = needsAdmin;
        }

        /// <summary>
        /// Unique name of setting
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Value of setting, initialized with default value
        /// </summary>
        public string Value { get; set; } = "";

        /// <summary>
        /// Description shown when using the help command
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Whether a user need admin to change this setting
        /// </summary>
        public bool needsAdmin { get; set; } = true;

        /// <summary>
        /// Alternative names of the command, will be checked if the main name does not fit. Not recommended to use too much.
        /// </summary>
        public List<string> CommandAlts { get; set; } = new List<string>();
    }
}
