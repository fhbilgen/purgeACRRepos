// https://learn.microsoft.com/en-us/azure/container-registry/quickstart-client-libraries?pivots=programming-language-csharp
// https://learn.microsoft.com/en-us/dotnet/api/overview/azure/container-registry?view=azure-dotnet
namespace purgeACRRepos
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {            
            var cred = ACRAuth.GetAzCredentials();            
            var client =  ACRAuth.ConnectToACR(cred);

            var acrEng = new ACREngine();
            await acrEng.GetRepos(client);
            acrEng.DisplayRepos();
            
            await acrEng.GetManifestCollections(client);
            acrEng.DisplayManifestsForEveryRepo();

            acrEng.DisplayDistinctDatesForEachRepo();

            //Console.WriteLine("Enter the repo name to display the images of the last N days");
            //var rpNm = Console.ReadLine();
            //Console.WriteLine("Enter the number of days");
            //var days = Int32.Parse(Console.ReadLine());
            //acrEng.DisplayTheImagesOfTheLastNDays(rpNm, days);

            Console.WriteLine("Enter the repo name to display the last N images");
            var rpNm = Console.ReadLine();
            Console.WriteLine("Enter the number of images");
            var images = Int32.Parse(Console.ReadLine());
            acrEng.DisplayTheLastNImages(rpNm, images);
            
            Console.WriteLine("Hit any key to exit");
            Console.ReadKey();
        }
    }
}