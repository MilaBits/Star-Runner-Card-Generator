using Card_Maker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using Card_Maker.Enums;

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

            foreach (CardItem item in CardLoader.LoadImages().OrderBy(x => x.CardData.Name))
            {
                lbCards.Items.Add(item);
            }

            foreach (var role in Enum.GetNames(typeof(Role)))
            {
                cbbRole.Items.Add(role);
            }


            tbSearch.TextChanged += tbSearch_TextChanged;
            slCost.ValueChanged += slCost_ValueChanged;
            cbbRole.SelectionChanged += cbbRole_SelectionChanged;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            DirectoryInfo cardDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"CardData\");

            SelectedCard = Card.LoadFromJson(CardLoader.filenames[lbCards.SelectedIndex]);

            DialogResult = true;

            CardLoader.filenames.Clear();
            Close();
        }

        private void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tbSearch.Text.Equals("Search...")) tbSearch.Text = "";
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filter();
        }

        private void Filter() {
            lbCards.Items.Clear();
            foreach (CardItem item in CardLoader.GetFilteredCards(tbSearch.Text, cbDescription.IsChecked, cbbRole.SelectedIndex, Convert.ToInt16(slCost.Value)).OrderBy(x => x.CardData.Name))
            {
                lbCards.Items.Add(item);
            }
        }

        private void slCost_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Filter();
            
            // Update cost label text
            if (slCost.Value == -1) {
                lbCost.Content = "Any";
                return;
            }
            lbCost.Content = slCost.Value;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            tbSearch.Text = "";
            cbbRole.SelectedIndex = -1;
            slCost.Value = -1;
            
            Filter();
        }

        private void cbbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
        }
    }
}
