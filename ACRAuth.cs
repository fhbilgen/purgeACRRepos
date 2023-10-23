using Azure.Containers.ContainerRegistry;
using Azure.Core;
using Azure.Identity;

namespace purgeACRRepos
{
    internal class ACRAuth
    {
        static ACRAuth()
        {
            Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", "b5730ee0-d8c6-4fad-8b2a-f8d8403e9e77");
            Environment.SetEnvironmentVariable("AZURE_TENANT_ID", "16b3c013-d300-468d-ac64-7eda0820b6d3");
            Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", "prk8Q~V-DR3I.DehbKGo0ENi4jgEOoUAj8ohaa-X");
        }
        public static DefaultAzureCredential GetAzCredentials()
        {
            // the includeInteractiveCredentials constructor parameter can be used to enable interactive authentication
            var credential = new DefaultAzureCredential();

            return credential;
            
        }

        //TODO:  get the values from configuration
        public static ContainerRegistryClient ConnectToACR(DefaultAzureCredential azDefCred)
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

            var client = new ContainerRegistryClient(new Uri("https://k8stestacreastus2.azurecr.io"), azDefCred, options);
            
            return client;
        }

        
    }
}
