using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace InventoryCount.App.ViewModels
{
    internal class MainPageViewModel : BaseViewModel
    {
        private string _area { get; set; }
        public MainPageViewModel() 
        {
            //Task.Run(async () => await AskForPermission());
        }

        public string Area
        {
            get { return _area; }
            set 
            { 
                _area = value; 
                OnPropertyChanged(nameof(Area));
            }
        }

        public ICommand ContinueCommand 
        { 
            get 
            {
                return new Command((e) =>
                {
                    Device.BeginInvokeOnMainThread(async () => 
                    {
                        int selectedArea = string.IsNullOrWhiteSpace(Area) ? 0 : int.Parse(Area);
                        var storageRead = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
                        if (storageRead != PermissionStatus.Granted)
                        {
                            var status = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
                            if (status == PermissionStatus.Granted)
                            {
                                await Application.Current.MainPage.Navigation.PushAsync(new Pages.ItemCount(selectedArea));
                            }
                            else
                            {
                                await Application.Current.MainPage.DisplayAlert("Permiso Necesario", "Permiso necesario para importar productos", "Ok");
                            }
                        }
                        else
                        {
                            await Application.Current.MainPage.Navigation.PushAsync(new Pages.ItemCount(selectedArea));
                        }
                    });
                });
            } 
        }
    }
}
