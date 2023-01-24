using InventoryCount.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace InventoryCount.App.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemCount : ContentPage
    {
        ItemCountViewModel _vm;
        public ItemCount(int area)
        {
            InitializeComponent();
            _vm = new ItemCountViewModel(area);
            this.BindingContext = _vm;
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var control = (Entry)sender;
            control.Focus();
        }

        private void Entry_Completed(object sender, EventArgs e)
        {
            _vm.ValidateTypedCode();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Task.Run(async () => {
                await Task.Delay(100);
                _vm.CapturedCode = string.Empty;
                _vm.IsExistingItem = false;
                Device.BeginInvokeOnMainThread(() => { this.entCode.Focus(); });
            });
        }
    }
}