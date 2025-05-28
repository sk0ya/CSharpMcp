using Microsoft.Extensions.Configuration;

namespace CreateMcpServer
{
    internal static class CreateMcpServerPath
    {
        private static string? rootFolderPath;

        public static string RootFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(rootFolderPath))
                {
                    LoadConfiguration();
                }

                return rootFolderPath!;
            }
        }

        private static void LoadConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            
            rootFolderPath = configuration["McpServer:RootFolderPath"];
            if (string.IsNullOrEmpty(rootFolderPath))
            {
                throw new ArgumentException("RootFolderPath not found in appsettings.json. Please set McpServer:RootFolderPath in the configuration file.");
            }
        }   
    }
}
