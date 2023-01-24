using InventoryCount.App.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Xml.Linq;

namespace InventoryCount.App
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = new MainPageViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Task.Run(async () => {
                await Task.Delay(100);
                Device.BeginInvokeOnMainThread(() => { this.entArea.Focus(); });
            });
        }
    }
}
