using Microsoft.Extensions.Configuration;

namespace TodoApi
{
    public static class LogConfiguration
    {
        private static IConfigurationRoot _configurationRoot;
        public static IConfigurationRoot ConfigurationRoot => _configurationRoot ?? (_configurationRoot = new ConfigurationBuilder()
                                                                  .AddJsonFile("config.json")
                                                                  .Build());
    }
}
