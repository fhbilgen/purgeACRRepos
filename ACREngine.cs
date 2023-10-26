using System.Collections.Generic;
using System.Linq;
using Azure;
using Azure.Containers.ContainerRegistry;
using Azure.Identity;
using Microsoft.Identity.Client;
using static System.Net.Mime.MediaTypeNames;

namespace purgeACRRepos
{
    public class ACREngine
    {
        
        private List<ContainerRepository> Repos { get; set; }
        private Dictionary<string, List<ArtifactManifestProperties>> Acr { get; set; }
        private ContainerRegistryClient Client { get; set; }
        
        
        
        public ACREngine(ContainerRegistryClient client)
        {            
            Client = client;
            Repos = new List<ContainerRepository>();
            Acr = new Dictionary<string, List<ArtifactManifestProperties>>();
        }

        public async Task GetRepos()
        {
            Repos.Clear();
            AsyncPageable<string> repositories = Client.GetRepositoryNamesAsync();
            await foreach (var repo in repositories)
            {
                Repos.Add(Client.GetRepository(repo));
            }
        }

        public ContainerRepository GetRepo(string repoName)
        {
            return Repos.Where(r => r.Name == repoName).FirstOrDefault();
        }

        public async Task GetManifestCollections()
        {
            Acr.Clear();
            foreach (var repo in Repos)
            {               
                AsyncPageable<ArtifactManifestProperties> imageManifests =
                   repo.GetAllManifestPropertiesAsync( ArtifactManifestOrder.LastUpdatedOnDescending);
                
                List<ArtifactManifestProperties> manifests = new List<ArtifactManifestProperties>();

                await foreach (var manifest in imageManifests)
                    manifests.Add(manifest);
                
                Acr.Add(repo.Name, manifests);
            }
        }

        public async Task InitACREngineAsync()
        {            
            await GetRepos();
            await GetManifestCollections();
        }

        public void DisplayRepos()
        {
            int i = 1;
           
            foreach (var repo in Repos)
                Console.WriteLine($"{i++}. Repo Name: {repo.Name} Manifest Count: {Acr[repo.Name].Count} EndPoint: {repo.RegistryEndpoint} ");
        }

        public void DisplayManifestsForEveryRepo()
        {
            int i = 1;

            foreach (var repo in Repos)
            {
                Console.WriteLine($"{i++}. Repo Name: {repo.Name}");
                DisplayManifests(repo.Name);
            }
        }

        public void DisplayManifests(string repoName)
        {
            int i = 1;
            
            foreach (var manifest in Acr[repoName])
            {
                Console.Write($"{i++}. ");
                DisplayManifest(manifest);
            }
        }

        public void DisplayManifests(List<ArtifactManifestProperties> manifests)
        {
            int i = 1;

            foreach (var manifest in manifests)
            {
                Console.Write($"{i++}. ");
                DisplayManifest(manifest);
            }
        }

        public void DisplayManifest(ArtifactManifestProperties manifest)
        {
            string tags = string.Join(",", manifest.Tags);

            Console.WriteLine($"Tags: {tags} Created: {manifest.CreatedOn.ToLocalTime()} Last Updated: {manifest.LastUpdatedOn.ToLocalTime()} " +
                $"OS: {manifest.OperatingSystem.ToString()} Size: {manifest.SizeInBytes} {manifest.Digest}");
        }

        public void DisplayDistinctDatesForEachRepo()
        {
            foreach (var repo in Repos)
            {
                Console.WriteLine($"Repo Name: {repo.Name}");
                DisplayTheLatestImageOfEachDay(repo.Name);
            }
        }
        public void DisplayTheLatestImageOfEachDay(string repoName)
        {
            DateTime dt = DateTime.Today.AddDays(1);
            List <ArtifactManifestProperties> remainings = new List<ArtifactManifestProperties>();
            List < ArtifactManifestProperties > goingtobepurged = new List<ArtifactManifestProperties>();

            foreach (var manifest in Acr[repoName])
                if (DateTime.Compare(manifest.LastUpdatedOn.Date, dt.Date) == 0)
                    goingtobepurged.Add(manifest);
                else
                {
                    dt = manifest.LastUpdatedOn.Date;
                    remainings.Add(manifest);
                }

            Console.WriteLine($"DisplayTheLatestImageOfEachDay for the repo {repoName}");
            DisplayTheManifestLists(remainings, goingtobepurged);
        }

        public void DisplayTheImagesOfTheLastNDays(string repoName, int dayCount)
        {
            int count = 0;
            DateTime dt = DateTime.Today.AddDays(1);
            List<ArtifactManifestProperties> remainings = new List<ArtifactManifestProperties>();
            List<ArtifactManifestProperties> goingtobepurged = new List<ArtifactManifestProperties>();

            foreach (var manifest in Acr[repoName])
            {
                if (DateTime.Compare(manifest.LastUpdatedOn.Date, dt.Date) != 0)
                {
                    dt = manifest.LastUpdatedOn.Date;
                    count++;
                }

                if ( count <= dayCount )
                    remainings.Add(manifest);
                else
                    goingtobepurged.Add(manifest);                
            }

            Console.WriteLine($"DisplayTheImagesOfTheLastNDays for the repo {repoName} for the {dayCount} days");
            DisplayTheManifestLists(remainings, goingtobepurged);
        }

       
        public void DisplayTheLastNImages(string repoName, int imageCount)
        {
            int count = 0;            
            List<ArtifactManifestProperties> remainings = new List<ArtifactManifestProperties>();
            List<ArtifactManifestProperties> goingtobepurged = new List<ArtifactManifestProperties>();

            foreach (var manifest in Acr[repoName])
            {               
                if (++count <= imageCount)
                    remainings.Add(manifest);
                else
                    goingtobepurged.Add(manifest);
            }

            Console.WriteLine($"DisplayTheImagesOfTheLastNDays for the repo {repoName} for the {imageCount} images");
            DisplayTheManifestLists(remainings, goingtobepurged);
        }

        public void DisplayTheManifestLists(List<ArtifactManifestProperties> remainings, List<ArtifactManifestProperties> goingtobepurged)
        {
            Console.WriteLine($"{remainings.Count} manifests will be kept. They are:");
            DisplayManifests(remainings);
            Console.WriteLine($"{goingtobepurged.Count} manifests will be purged. They are:");
            DisplayManifests(goingtobepurged);
        }


        public async Task DeleteAllButTheLatestImageOfEachDay(string repoName)
        {
            DateTime dt = DateTime.Today.AddDays(1);
            List<ArtifactManifestProperties> remainings = new List<ArtifactManifestProperties>();
            List<ArtifactManifestProperties> goingtobepurged = new List<ArtifactManifestProperties>();

            foreach (var manifest in Acr[repoName])
                if (DateTime.Compare(manifest.LastUpdatedOn.Date, dt.Date) == 0)
                    goingtobepurged.Add(manifest);
                else
                {
                    dt = manifest.LastUpdatedOn.Date;
                    remainings.Add(manifest);
                }                     

            await DeleteManifestsAsync(repoName, goingtobepurged);
        }

        public async Task DeleteTheImagesExceptTheLastNDays(string repoName, int dayCount)
        {
            int count = 0;
            DateTime dt = DateTime.Today.AddDays(1);
            List<ArtifactManifestProperties> remainings = new List<ArtifactManifestProperties>();
            List<ArtifactManifestProperties> goingtobepurged = new List<ArtifactManifestProperties>();

            foreach (var manifest in Acr[repoName])
            {
                if (DateTime.Compare(manifest.LastUpdatedOn.Date, dt.Date) != 0)
                {
                    dt = manifest.LastUpdatedOn.Date;
                    count++;
                }

                if (count <= dayCount)
                    remainings.Add(manifest);
                else
                    goingtobepurged.Add(manifest);
            }
            
            await DeleteManifestsAsync(repoName, goingtobepurged);
        }

        public async Task DeleteAllButTheLastNImages(string repoName, int imageCount)
        {
            int count = 0;
            List<ArtifactManifestProperties> remainings = new List<ArtifactManifestProperties>();
            List<ArtifactManifestProperties> goingtobepurged = new List<ArtifactManifestProperties>();

            foreach (var manifest in Acr[repoName])
            {
                if (++count <= imageCount)
                    remainings.Add(manifest);
                else
                    goingtobepurged.Add(manifest);
            }

            await DeleteManifestsAsync(repoName, goingtobepurged);
        }

        private async Task DeleteManifestsAsync(string repoName, List<ArtifactManifestProperties> manifests)
        {
            ContainerRepository repo = GetRepo(repoName);
            foreach (var manifest in manifests)
                await DeleteManifestAsync(repo, manifest);

            await GetManifestCollections();
        }


        public async Task DeleteManifestAsync(ContainerRepository repo, ArtifactManifestProperties manifest)
        {
            RegistryArtifact artifact = repo.GetArtifact(manifest.Digest);
            Console.WriteLine($"Deleting image with digest {manifest.Digest}.");
            Console.WriteLine($"   Deleting the following tags from the image: ");
            foreach (var tagName in manifest.Tags)
            {
                Console.WriteLine($"        {manifest.RepositoryName}:{tagName}");
                await artifact.DeleteTagAsync(tagName);
            }
            await artifact.DeleteAsync();            
        }

    }
}

