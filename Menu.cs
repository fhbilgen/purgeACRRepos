using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace purgeACRRepos
{
    internal class MenuItem
    {
        public string Title { get; set; }
        public int Id { get; set; }

        public ConsoleColor Color { get; set; }

        public MenuItem(string title, int id, ConsoleColor color)
        {
            Title = title;
            Id = id;
            Color = color;
        }
    }
    public static class Menu
    {
        private static List<MenuItem> MenuItems = new List<MenuItem>();

        static Menu()
        {
            MenuItems.Add(new MenuItem("ACR Bağlan", 1, ConsoleColor.Green ));
            MenuItems.Add(new MenuItem("Repoları listele", 2, ConsoleColor.Green));
            MenuItems.Add(new MenuItem("Repo içeriğini listele", 3, ConsoleColor.Green));
            MenuItems.Add(new MenuItem("Repodaki her günkü imajların son yüklenenlerini listele", 4, ConsoleColor.Green));
            MenuItems.Add(new MenuItem("Repoya son N günde yüklenmiş imajları listele", 5, ConsoleColor.Green));
            MenuItems.Add(new MenuItem("Repoya yüklenmiş son X adet imajı listele", 6, ConsoleColor.Green));

            MenuItems.Add(new MenuItem("Repodaki her günkü imajların son yüklenenleri dışındakileri sil!!!", 7, ConsoleColor.Red));
            MenuItems.Add(new MenuItem("Repoya son N günden önce yüklenmiş imajları sil!!!", 8, ConsoleColor.Red));
            MenuItems.Add(new MenuItem("Repoya yüklenmiş son X adet öncesi imajları sil!!!", 9, ConsoleColor.Red));

            MenuItems.Add(new MenuItem("Çıkış", 0, ConsoleColor.Green));
        }

        public static void Display()
        {
            Console.WriteLine($"{Environment.NewLine} {Environment.NewLine}");
            foreach(var menuItem in MenuItems)
            {
                Console.ForegroundColor = menuItem.Color;
                Console.WriteLine("{0,2} {1}", menuItem.Id, menuItem.Title);
            }
            Console.ResetColor();
        }

        public static int GetChoice()
        {
            var i = 0;
            try
            {                
                i = int.Parse(Console.ReadLine());
            }
            catch (NullReferenceException)
            {
                i = 0;
            }
            
            return i;
        }

    }
}
