using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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

        public CardItem(BitmapImage image, string filename) {
            Image = image;
            Filename = filename;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
