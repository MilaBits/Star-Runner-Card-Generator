using Card_Maker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Card_Maker.Screens
{
    /// <summary>
    /// Interaction logic for Cards.xaml
    /// </summary>
    public partial class Cards : Window
    {

        public Card SelectedCard { get; private set; }

        public Cards()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            DirectoryInfo cardDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"CardData\");

            SelectedCard = Card.LoadFromJson(ImageLoader.filenames[lbCards.SelectedIndex]);

            DialogResult = true;

            ImageLoader.filenames.Clear();
            Close();
        }
    }
}
