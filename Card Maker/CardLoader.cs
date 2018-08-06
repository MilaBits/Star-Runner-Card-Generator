using Card_Maker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Card_Maker.Enums;

namespace Card_Maker
{
    public static class CardLoader
    {
        public static List<string> filenames = new List<string>();
        private static List<CardItem> LoadedCards = new List<CardItem>();

        public static List<CardItem> LoadImages()
        {
            List<CardItem> items = new List<CardItem>();

            DirectoryInfo imageDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"Cards\");

            List<FileInfo> imageFiles = new List<FileInfo>();
            imageFiles.AddRange(imageDir.GetFiles("*.png"));
            imageFiles.AddRange(imageDir.GetFiles("*.jpg"));

            foreach (FileInfo imageFile in imageFiles)
            { //TODO: support more formats
                Uri uri = new Uri(imageFile.FullName);

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.UriSource = uri;
                image.EndInit();

                string filename = Path.GetFileNameWithoutExtension(uri.AbsolutePath);
                filenames.Add(filename);

                items.Add(new CardItem(image, filename.Replace("%20", " "), getCardData(imageFile)));
                LoadedCards.Add(items[items.Count - 1]);
            }

            return items;
        }

        private static Card getCardData(FileInfo imageFile)
        {
            string path = imageFile.FullName.Split('.')[0] + ".card";
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Card>(json);
        }

        public static List<CardItem> GetCards()
        {
            IEnumerable<CardItem> collection = LoadedCards;
            List<CardItem> cards = new List<CardItem>(collection);

            return cards;
        }

        public static List<CardItem> GetFilteredCards(string search)
        {
            return LoadedCards.FindAll(x => x.CardData.Name.ToLower().Contains(search.ToLower()));
        }

        public static List<CardItem> GetFilteredCards(Role role)
        {
            return LoadedCards.FindAll(x => x.CardData.Roles.Any(r => r.Equals(role)));
        }

        public static List<CardItem> GetFilteredCards(string search, bool? description, int role, int cost)
        {
            List<CardItem> items = LoadedCards;

            if (!string.IsNullOrWhiteSpace(search))
            {
                if (description != null && (bool)description)
                {
                    items = items.Where(x =>
                        x.CardData.Name.ToLower().Contains(search.ToLower()) || x.CardData.Description.ToLower().Contains(search.ToLower())).ToList();
                }
                else
                {
                    items = items.Where(x => x.CardData.Name.ToLower().Contains(search.ToLower())).ToList();
                }
            }

            if (role > -1 && role != 6) items = items.Where(x => x.CardData.Roles.Any(r => r.Equals((Role)role))).ToList();

            if (cost > -1) items = items.Where(x => x.CardData.Cost == cost).ToList();

            return items;
        }
    }
}