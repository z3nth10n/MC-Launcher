using z3nth10n_Launcher.Properties;

namespace z3nth10n_Launcher
{
    public static class SM
    {
        private readonly static Settings settings = Settings.Default;

        public static string Username
        {
            get
            {
                return settings.Username;
            }
            set
            {
                settings.Username = value;
                settings.Save();
            }
        }

        public static void Save()
        {
            settings.Save();
        }
    }
}