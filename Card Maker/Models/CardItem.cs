using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace Card_Maker.Models {
    public class CardItem {
        private BitmapImage _image;
        public BitmapImage Image {
            get { return _image; }
            set {
                _image = value;
                NotifyPropertyChanged();
            }
        }

        private string _filename;
        public string Filename {
            get { return _filename; }
            set {
                _filename = value;
                NotifyPropertyChanged();
            }
        }

        private Card _cardData;
        public Card CardData {
            get { return _cardData; }
            set {
                _cardData = value;
                NotifyPropertyChanged();
            }
        }

        public CardItem(BitmapImage image, string filename, Card cardData) {
            Image = image;
            Filename = filename;
            CardData = cardData;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
