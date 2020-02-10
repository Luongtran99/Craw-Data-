using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawDataWPF
{
    class MenuTreeView
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public ObservableCollection<MenuTreeView> Items { get; set; }
        public MenuTreeView()
        {
            this.Items = new ObservableCollection<MenuTreeView>();
        }
    }
}
