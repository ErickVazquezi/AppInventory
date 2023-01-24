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
    public partial class AddItemCount : ContentPage
    {
        private ItemCountViewModel _vm;
        public AddItemCount(ItemCountViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            this.BindingContext= _vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Task.Run(async () => {
                await Task.Delay(100);
                Device.BeginInvokeOnMainThread(() => { this.entCount.Focus(); });
            });
        }
    }
}