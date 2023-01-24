using InventoryCount.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace InventoryCount.App.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchItem : ContentPage
    {
        private ItemCountViewModel _vm = null;
        public SearchItem(ItemCountViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            this.BindingContext = _vm;
        }

        private void cvSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            cvStoreItems.ItemsSource = _vm.StoreItems.Where(i => i.Code.Contains(cvSearchBar.Text) || i.Name.ToLower().Contains(cvSearchBar.Text));
        }
    }
}