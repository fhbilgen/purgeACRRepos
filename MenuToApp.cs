using Azure.Containers.ContainerRegistry;
using Azure.Identity;
using Azure.ResourceManager.ContainerRegistry.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace purgeACRRepos
{
    internal class MenuToApp
    {
        
        //private ContainerRegistryClient ACRCli { get; set; }
        private IConfiguration Config { get; set; }
        private ACREngine ACREng {get  ; set;}
        private ACRAuth Auth { get; set; }


        public MenuToApp(IConfiguration cfgRoot)
        {
            Config = cfgRoot;
            Auth = new ACRAuth(Config);
        }

        public async Task InitAsync()
        {            
            ContainerRegistryClient acrCli = Auth.ConnectToACR();
            ACREng = new ACREngine(acrCli);
            await ACREng.InitACREngineAsync();
        }

        public async Task MenuLoop()
        {
            var choice = -1;
            while (choice != 0)
            {
                Menu.Display();
                choice = Menu.GetChoice();
                await ExecuteChoice(choice);
            }            
        }

        private bool Confirm()
        {
            Console.WriteLine("Silme işlemine devam etmek istiyor musunuz?(E/H)");
            var ch = Console.ReadKey().KeyChar;
            Console.ReadLine();
            if (ch == 'E' || ch == 'e')
                return true;
            else
                return false;

        }

        public async Task ExecuteChoice(int choice)
        {
            string repoName;
            int days = 0, count = 0;
            switch (choice)
            {
                case 0:
                    break;
                case 1:
                    Console.Write("Yeni ACR Sunucusu: ");
                    var srv = Console.ReadLine();
                    Auth.SetACRServerInfo(srv);
                    await InitAsync();
                    break;

                case 2:
                    ACREng.DisplayRepos();
                    break;

                case 3:
                    Console.Write("Repo adı: ");
                    repoName = Console.ReadLine();
                    ACREng.DisplayManifests(repoName);
                    break;

                case 4:
                    Console.Write("Repo adı: ");
                    repoName = Console.ReadLine();
                    ACREng.DisplayTheLatestImageOfEachDay(repoName);
                    break;

                case 5:
                    Console.Write("Repo adı: ");
                    repoName = Console.ReadLine();
                    Console.Write("Son N gün sayısı: ");
                    days = Int32.Parse(Console.ReadLine());
                    ACREng.DisplayTheImagesOfTheLastNDays(repoName, days);
                    break;

                case 6:
                    Console.Write("Repo adı: ");
                    repoName = Console.ReadLine();
                    Console.Write("Yüklenen son imaj sayısı: ");
                    count = Int32.Parse(Console.ReadLine());
                    ACREng.DisplayTheLastNImages(repoName, count);
                    break;


                // DELETE OPERATIONS
                case 7:
                    Console.Write("Repo adı: ");
                    repoName = Console.ReadLine();
                    
                    ACREng.DisplayTheLatestImageOfEachDay(repoName);
                    if (Confirm())
                        await ACREng.DeleteAllButTheLatestImageOfEachDay(repoName);

                    break;

                case 8:
                    Console.Write("Repo adı: ");
                    repoName = Console.ReadLine();
                    Console.Write("Son N gün sayısı: ");
                    days = Int32.Parse(Console.ReadLine());
                    
                    ACREng.DisplayTheImagesOfTheLastNDays(repoName, days);
                    if (Confirm())
                        await ACREng.DeleteTheImagesExceptTheLastNDays(repoName, days);
                    
                    break;

                case 9:
                    Console.Write("Repo adı: ");
                    repoName = Console.ReadLine();
                    Console.Write("Yüklenen son imaj sayısı: ");
                    count = Int32.Parse(Console.ReadLine());                    
                    
                    ACREng.DisplayTheLastNImages(repoName, count);
                    if (Confirm())
                        await ACREng.DeleteAllButTheLastNImages(repoName, count);

                    break;
                default:
                    break;
            }
        }
    }
}
