namespace DSharpPlus.SettingsManager
{
    public class SettingEntity
    {

        public SettingEntity() { }
        public SettingEntity(string Name, string Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

        public SettingEntity(string name, string value, string description, bool needsAdmin)
        {
            Name = name;
            Value = value;
            Description = description;
            this.needsAdmin = needsAdmin;
        }

        public string Name { get; set; } = "";
        public string Value { get; set; } = "";

        public string Description { get; set; } = "";

        public bool needsAdmin { get; set; } = true;

        public List<string> CommandAlts { get; set; } = new List<string>();
    }
}
