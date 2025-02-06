using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTracker.Tests.Helper
{
    public static class SetConfigurationManager
    {
        private static void SetConfig(AppSettingsSection appSettings)
        {
            ConfigurationManager.AppSettings["SCREEPS_API_URL"] = appSettings.Settings["SCREEPS_API_URL"].Value;
            ConfigurationManager.AppSettings["SCREEPS_API_TOKEN"] = appSettings.Settings["SCREEPS_API_TOKEN"].Value;
            ConfigurationManager.AppSettings["SCREEPS_API_USERNAME"] = appSettings.Settings["SCREEPS_API_USERNAME"].Value;
            ConfigurationManager.AppSettings["SCREEPS_API_PASSWORD"] = appSettings.Settings["SCREEPS_API_PASSWORD"].Value;
            if (appSettings.Settings.AllKeys.Contains("SCREEPS_SHARDNAME")) 
                ConfigurationManager.AppSettings["SCREEPS_SHARDNAME"] = appSettings.Settings["SCREEPS_SHARDNAME"].Value;
        }
        public static void SetPrivateConfig()
        {
            var configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = "App.Private.Config"
            };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            if (configuration.AppSettings != null) SetConfig(configuration.AppSettings);
        }
        public static void SetLiveConfig()
        {
            var configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = "App.Live.Config"
            };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            if (configuration.AppSettings != null) SetConfig(configuration.AppSettings);
        }
    }
}
