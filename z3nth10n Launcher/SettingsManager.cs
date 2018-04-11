using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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

        public static StringCollection Nicks
        {
            get
            {
                return settings.Nicks;
            }
        }

        public static void AddNick(string nick)
        {
            if (Nicks == null)
            {
                settings.Nicks = new StringCollection();
                settings.Save();
            }

            if (!Nicks.Contains(nick))
            {
                Nicks.Add(nick);
                settings.Save();
            }
        }

        public static void Save()
        {
            settings.Save();
        }
    }
}