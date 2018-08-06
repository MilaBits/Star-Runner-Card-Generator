using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Maker
{
    public static class LinqHelper
    {
        public static ObservableCollection<T> PopulateFrom<T>(this ObservableCollection<T> collection, IEnumerable<T> range)
        {
            foreach (var x in range)
            {
                collection.Add(x);
            }
            return collection;
        }
    }
}
