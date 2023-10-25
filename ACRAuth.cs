using Azure.Containers.ContainerRegistry;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace purgeACRRepos
{
    internal class ACRAuth
    {
        private static string[] _envVars = { "AZURE_CLIENT_ID", "AZURE_TENANT_ID", "AZURE_CLIENT_SECRET", "ACR_LOGIN_SERVER" };
        public string ACRServer { get; set; }
        
        private DefaultAzureCredential azCred { get; set; }

        public IConfiguration Config { get; set; }

        public ACRAuth(IConfiguration config)
        {
            //Environment.SetEnvironmentVariable(_envVars[0], "b5730ee0-d8c6-4fad-8b2a-f8d8403e9e77");
            //Environment.SetEnvironmentVariable(_envVars[1], "16b3c013-d300-468d-ac64-7eda0820b6d3");
            //Environment.SetEnvironmentVariable(_envVars[2], "prk8Q~V-DR3I.DehbKGo0ENi4jgEOoUAj8ohaa-X");
            
            Config = config;

            azCred = GetAzCredentials(0);

            if (azCred == null)
            {
                // Env var tanımlı değil appsettings.json ve komut satırı argümanları denenecek
                azCred = GetAzCredentials(1);
                if (azCred == null)
                {
                    // appsettings.json ve komut satırı argümanları da tanımlı değil
                    // interaktif olarak bilgiler alınacak
                    //azCred = GetAzCredentials(2);
                    //if (azCred == null)
                    //{
                        throw new Exception("Credential bilgileri geçerli değil");
                    //}
                }
            }
        }

        // Either command line or appsettings.json
        private bool ValuesExistInConfig()
        {
            var clientId = Config[_envVars[0]];
            var tenantId = Config[_envVars[1]];
            var clientSecret = Config[_envVars[2]];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientSecret))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool EnvVarsExist()
        {
            var clientId = Environment.GetEnvironmentVariable(_envVars[0]);
            var tenantId = Environment.GetEnvironmentVariable(_envVars[1]);
            var clientSecret = Environment.GetEnvironmentVariable(_envVars[2]);

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientSecret))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void SetEnvVars(string clientId, string tenantId, string clientSecret)
        {
            Environment.SetEnvironmentVariable(_envVars[0], clientId);
            Environment.SetEnvironmentVariable(_envVars[1], tenantId);
            Environment.SetEnvironmentVariable(_envVars[2], clientSecret);
        }

        public void ResetEnvVars()
        {
            Environment.SetEnvironmentVariable(_envVars[0], null);
            Environment.SetEnvironmentVariable(_envVars[1], null);
            Environment.SetEnvironmentVariable(_envVars[2], null);
        }

        public DefaultAzureCredential GetAzCredentials(int source)
        {
            DefaultAzureCredential credential = null;

            switch (source)
            {
                // from existing env vars
                case 0:
                    credential = GetAzCredFromEnv();
                    break;
                
                //from appsettings.json or command line
                case 1:
                    credential = GetAzCredFromConfig();
                    break;

                    // from interactive values
                //case 2:
                //    credential = GetAzCredFromInteractive();
                //    break;
                
                default:
                    break;
            }

            return credential;
            
        }

        public DefaultAzureCredential GetAzCredFromEnv()
        {
            if (EnvVarsExist())
                return new DefaultAzureCredential();
            else
                return null;
        }


        public DefaultAzureCredential GetAzCredFromConfig()
        {
            if (ValuesExistInConfig())
            {
                ResetEnvVars();
                var clientId = Config[_envVars[0]];
                var tenantId = Config[_envVars[1]];
                var clientSecret = Config[_envVars[2]];
                SetEnvVars(clientId, tenantId, clientSecret);
                return new DefaultAzureCredential();
            }                
            else
                return null;
        }

        //public DefaultAzureCredential GetAzCredFromInteractive()
        //{
            
        //    ResetEnvVars();
        //    string clientId, tenantId, clientSecret;

        //    Console.Write("Client id: ");
        //    clientId = Console.ReadLine();
        //    Console.Write("Tenant id: ");
        //    tenantId = Console.ReadLine();
        //    Console.Write("Client secret: ");
        //    clientSecret = Console.ReadLine();

        //    SetEnvVars(clientId, tenantId, clientSecret);
        //    return new DefaultAzureCredential();
            
        //}

        public void SetAcrLoginServer()
        {
            ACRServer = Config[_envVars[3]];
            Console.WriteLine($"ACR Sunucusu: {ACRServer}");
            Console.Write("Sunucuyu değiştirmek ister misiniz?(E/H)");
            var ch = Console.ReadKey().KeyChar;
            
            if ( ch == 'E' || ch == 'e')
            {
                Console.Write("Yeni ACR Sunucusu: ");
                ACRServer = Console.ReadLine();
            }
        }

        private void SetACRServerInfo()
        {
            ACRServer = Config[_envVars[3]];

            if (ACRServer == null)
                ACRServer = Environment.GetEnvironmentVariable(_envVars[3]);
            else
                return;

            if (ACRServer == null)
                throw new Exception("ACR sunucu bilgisini komut satırı, appsettings.json dosyası veya çevre değişkeninde tanımlamış olmanız gerekir");
            else
                return;

        }

        public void SetACRServerInfo(string acrServer)
        {
            ACRServer = acrServer;
        }

        //TODO:  get the values from configuration
        public ContainerRegistryClient ConnectToACR()
        {
                        
            ContainerRegistryClientOptions options = new ContainerRegistryClientOptions()
            {
                Retry =
                {
                         Delay= TimeSpan.FromSeconds(2),
                         MaxDelay = TimeSpan.FromSeconds(8),
                         MaxRetries = 3,
                         Mode = RetryMode.Exponential
                        }
            };

            SetACRServerInfo();
            var client = new ContainerRegistryClient(new Uri(ACRServer), azCred, options);
            
            return client;
        }

        
    }
}
