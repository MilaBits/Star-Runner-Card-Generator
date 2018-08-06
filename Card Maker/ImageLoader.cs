using Card_Maker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace Card_Maker {
    public static class ImageLoader {

        public static List<string> filenames = new List<string>();

        public static List<CardItem> LoadImages() {
            List<CardItem> items = new List<CardItem>();

            DirectoryInfo imageDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"Cards\");
            foreach (FileInfo imageFile in imageDir.GetFiles("*.png")) {
                Uri uri = new Uri(imageFile.FullName);

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.UriSource = uri;
                image.EndInit();
                
                string filename = Path.GetFileNameWithoutExtension(uri.AbsolutePath);
                filenames.Add(filename);

                items.Add(new CardItem(image, filename.Replace("%20"," ")));
            }

            return items;
        }
    }
}
