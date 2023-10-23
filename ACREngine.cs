using System.Collections.Generic;
using System.Linq;
using Azure;
using Azure.Containers.ContainerRegistry;
using Azure.Identity;
using Microsoft.Identity.Client;

namespace purgeACRRepos
{
    public class ACREngine
    {
        private List<ContainerRepository> _repos = new List<ContainerRepository>();
        private Dictionary<string, List<ArtifactManifestProperties>> _acr = new Dictionary<string, List<ArtifactManifestProperties>>();
        public async Task GetRepos(ContainerRegistryClient client)
        {
            _repos.Clear();
            AsyncPageable<string> repositories = client.GetRepositoryNamesAsync();
            await foreach (var repo in repositories)
            {
                _repos.Add(client.GetRepository(repo));
            }
        }

        public async Task GetManifestCollections(ContainerRegistryClient client)
        {
            _acr.Clear();
            foreach (var repo in _repos)
            {               
                AsyncPageable<ArtifactManifestProperties> imageManifests =
                   repo.GetAllManifestPropertiesAsync( ArtifactManifestOrder.LastUpdatedOnDescending);
                
                List<ArtifactManifestProperties> manifests = new List<ArtifactManifestProperties>();

                await foreach (var manifest in imageManifests)
                    manifests.Add(manifest);
                
                _acr.Add(repo.Name, manifests);
            }
        }

        public void DisplayRepos()
        {
            int i = 1;
           
            foreach (var repo in _repos)
                Console.WriteLine($"{i++}. Repo Name: {repo.Name} EndPoint: {repo.RegistryEndpoint} ");
        }

        public void DisplayManifestsForEveryRepo()
        {
            int i = 1;

            foreach (var repo in _repos)
            {
                Console.WriteLine($"{i++}. Repo Name: {repo.Name}");
                DisplayManifests(repo.Name);
            }
        }

        public void DisplayManifests(string repoName)
        {
            int i = 1;
            
            foreach (var manifest in _acr[repoName])
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
            foreach (var repo in _repos)
            {
                Console.WriteLine($"Repo Name: {repo.Name}");
                DisplayTheLatestImageOfEachDay(repo.Name);
            }
        }
        public void DisplayTheLatestImageOfEachDay(string repoName)
        {
            DateTime dt = DateTime.Today;
            List <ArtifactManifestProperties> remainings = new List<ArtifactManifestProperties>();
            List < ArtifactManifestProperties > goingtobepurged = new List<ArtifactManifestProperties>();

            foreach (var manifest in _acr[repoName])
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
            DateTime dt = DateTime.Today;
            List<ArtifactManifestProperties> remainings = new List<ArtifactManifestProperties>();
            List<ArtifactManifestProperties> goingtobepurged = new List<ArtifactManifestProperties>();

            foreach (var manifest in _acr[repoName])
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

            foreach (var manifest in _acr[repoName])
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

    }
}
