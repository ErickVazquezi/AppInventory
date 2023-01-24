using CsvHelper;
using CsvHelper.Configuration;
using InventoryCount.App.Models;
using InventoryCount.App.Pages;
using InventoryCount.App.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using ZXing;
using ZXing.Net.Mobile.Forms;
using ZXing.OneD;

namespace InventoryCount.App.ViewModels
{
    public class ItemCountViewModel : BaseViewModel
    {
        private string _pageTitle;
        private ObservableCollection<InventoryItem> _inventoryCount = new ObservableCollection<InventoryItem>();
        private ObservableCollection<InventoryItem> _storeItems = new ObservableCollection<InventoryItem>();
        private string _selectedItemCount = string.Empty;
        private InventoryItem _currentItem;
        readonly dbInventory _dbContext;
        private int _totalInventory;
        private int _totalItemsCount;
        private int _area;
        private string _capturedCode = string.Empty;
        private bool _isExistingItem = false;
        public ItemCountViewModel(int area)
        {
            _area = area;
            PageTitle = $"Area: {_area}";
            _dbContext = dbInventory.Instance.GetAwaiter().GetResult();
            Task.Run(async () => { await LoadInventoryItems(); });
            LoadStoreItems();
        }

        public string PageTitle
        {
            get { return _pageTitle; }
            set 
            { 
                _pageTitle = value; 
                OnPropertyChanged(nameof(PageTitle));
            }
        }

        public ObservableCollection<InventoryItem> InventoryCount
        {
            get 
            {
                return _inventoryCount; 
            }
            set 
            { 
                _inventoryCount = value; 
                OnPropertyChanged(nameof(InventoryCount));
            }
        }

        public ObservableCollection<InventoryItem> StoreItems
        {
            get{ return _storeItems; }
            set
            {
                _storeItems = value;
                OnPropertyChanged(nameof(StoreItems));
            }
        }

        public string SelectedItemCount
        {
            get { return _selectedItemCount; }
            set
            {
                _selectedItemCount = value;
                OnPropertyChanged(nameof(SelectedItemCount));
            }
        }

        public InventoryItem CurrentItem
        {
            get { return _currentItem; }
            set
            {
                _currentItem = value;
                OnPropertyChanged(nameof(_currentItem));
            }
        }

        public int TotalItemsCount
        {
            get { return _totalItemsCount; }
            set
            {
                _totalItemsCount = value;
                OnPropertyChanged(nameof(TotalItemsCount));
            }
        }

        public int TotalInventory
        {
            get { return _totalInventory; }
            set
            {
                _totalInventory = value;
                OnPropertyChanged(nameof(TotalInventory));
            }
        }

        public string CapturedCode
        {
            get { return _capturedCode; }
            set
            {
                _capturedCode = value;
                OnPropertyChanged(nameof(CapturedCode));
            }
        }

        public bool IsExistingItem
        {
            get { return _isExistingItem; }
            set
            {
                _isExistingItem = value;
                OnPropertyChanged(nameof(IsExistingItem));
            }
        }

        public ICommand ScanCommand 
        { 
            get 
            {
                return new Command(async () =>
                {
                    var scanPage = new ZXingScannerPage() 
                    {
                        DefaultOverlayTopText = "Centra el codigo en la pantalla",
                        DefaultOverlayBottomText = "Escaneando",
                        DefaultOverlayShowFlashButton = true
                    };

                    scanPage.OnScanResult += (result) => 
                    {
                        // Stop scanning
                        scanPage.IsScanning = false;
                        // Pop the page and show the result
                        Device.BeginInvokeOnMainThread(async () => 
                        {
                            await Application.Current.MainPage.Navigation.PopAsync();
                            CurrentItem = StoreItems.FirstOrDefault(i => i.Code == result.Text);
                            if (CurrentItem != null)
                            {
                                if (CurrentItem.Id != 0 && InventoryCount.Any(i => i.Id == CurrentItem.Id))
                                    await Application.Current.MainPage.DisplayAlert("", "Producto ya capturado. Puedes editar la cantidad capturada", "Aceptar");

                                await Application.Current.MainPage.Navigation.PushAsync(new AddItemCount(this));
                            }
                            else
                            {
                                await Application.Current.MainPage.DisplayAlert(string.Empty, "Código no encontrado o escaneo incorrecto. Intenta buscarlo manualmente.", "Aceptar");
                                await Application.Current.MainPage.Navigation.PushAsync(new SearchItem(this));
                            }
                        });
                    };

                    // Navigate to our scanner page
                    await Application.Current.MainPage.Navigation.PushAsync(scanPage);
                });
            }
        }

        public void ValidateTypedCode()
        {
            // Pop the page and show the result
            Device.BeginInvokeOnMainThread(async () =>
            {
                CurrentItem = StoreItems.FirstOrDefault(i => i.Code == CapturedCode);
                if (CurrentItem != null)
                {
                    if (CurrentItem.Id != 0 && InventoryCount.Any(i => i.Id == CurrentItem.Id))
                    {
                        IsExistingItem = true;
                        await Application.Current.MainPage.DisplayAlert("", "Producto ya capturado. Puedes editar la cantidad capturada", "Aceptar");
                    }

                    await Application.Current.MainPage.Navigation.PushAsync(new AddItemCount(this));
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(string.Empty, "Código no encontrado o escaneo incorrecto. Intenta buscarlo manualmente.", "Aceptar");
                    await Application.Current.MainPage.Navigation.PushAsync(new SearchItem(this));
                }
            });
        }

        public ICommand EditCountCommand
        {
            get
            {
                return new Command(async (e) =>
                {
                    CurrentItem = (InventoryItem)e;
                    await Application.Current.MainPage.Navigation.PushAsync(new AddItemCount(this));
                });
            }
        }

        public ICommand GoSearchPageCommand
        {
            get
            {
                return new Command(async () =>
                {                   
                    await Application.Current.MainPage.Navigation.PushAsync(new SearchItem(this));
                });
            }
        }

        public ICommand SelectedItemCommand
        {
            get
            {
                return new Command(async (e) =>
                {
                    CurrentItem = (InventoryItem)e;
                    if (InventoryCount.Any(i => i.Code == CurrentItem.Code))
                    {
                        IsExistingItem = true;
                        await Application.Current.MainPage.DisplayAlert("", "Producto ya capturado. Puedes editar la cantidad capturada", "Aceptar");
                    }
                    
                    await Application.Current.MainPage.Navigation.PushAsync(new AddItemCount(this));
                });
            }
        }

        public Command SaveItemCountCommand
        {
            get
            {
                return new Command<InventoryItem>(async (e) =>
                {
                    try
                    {
                        var selectedItem = CurrentItem;
                        var dbValue = await _dbContext.GetItemAsync(selectedItem.Code);

                        if (CurrentItem.Count > 0 && IsExistingItem)
                        {
                            int selectedCount = !string.IsNullOrWhiteSpace(SelectedItemCount) ? int.Parse(SelectedItemCount) : 0;
                            selectedItem.Count += selectedCount;
                        }
                        else
                        {
                            selectedItem.Count = !string.IsNullOrWhiteSpace(SelectedItemCount) ? int.Parse(SelectedItemCount) : 0;
                        }
                        IsExistingItem = false;
                        selectedItem.Id = dbValue != null ? dbValue.Id : 0;
                        selectedItem.Area = _area;

                        var rowCount = await _dbContext.SaveItemAsync(selectedItem);

                        if (rowCount > 0)
                        {
                            if (InventoryCount.Count == 1 && InventoryCount.Any(i => i.Id == 0))
                                InventoryCount.Clear();

                            await LoadInventoryItems();

                            var page = Application.Current.MainPage.Navigation.NavigationStack.Where(p => p.GetType() == typeof(SearchItem)).FirstOrDefault();
                            if (page != null)
                                Application.Current.MainPage.Navigation.RemovePage(page);

                            await Application.Current.MainPage.Navigation.PopAsync();
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Error", "Ocurrió un error al guardar la información", "Aceptar");
                        }
                        SelectedItemCount = string.Empty;
                    }
                    catch (Exception)
                    {
                        SelectedItemCount = "";
                        await Application.Current.MainPage.DisplayAlert("Error", "Ingresa una cantidad válida", "Aceptar");

                    }
                });
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (StoreItems.Count <= 1)
                        LoadStoreItems();

                    await Application.Current.MainPage.Navigation.PushAsync(new SearchItem(this));
                });
            }
        }

        public ICommand DeleteInventoryCommand
        {
            get
            {
                return new Command(async () =>
                {
                    var deleteDialog = await Application.Current.MainPage.DisplayAlert("", "¿Estas seguro de eliminar los registros de esta área?", "Aceptar", "Cancelar");
                    if (deleteDialog)
                    {
                        foreach (var item in InventoryCount)
                            await _dbContext.DeleteItemAsync(item);

                        TotalInventory = 0;
                        TotalItemsCount = 0;
                        InventoryCount.Clear();
                    }
                });
            }
        }

        public ICommand DeleteItemCommand
        {
            get
            {
                return new Command(async (e) =>
                {
                    CurrentItem = (InventoryItem)e;
                    bool userConfirmation = await Application.Current.MainPage.DisplayAlert("", "¿Deseas eliminar este artículo?", "Aceptar", "Cancelar");
                    if (userConfirmation)
                    {
                        int affectedRows = await _dbContext.DeleteItemAsync(CurrentItem);
                        if (affectedRows <= 0)
                            await Application.Current.MainPage.DisplayAlert("", "Ocurrió un error al intentar eliminar el registro", "Aceptar");
                        else
                            await LoadInventoryItems();                       
                    }
                });
            }
        }

        public ICommand ExportInventoryCommand
        {
            get
            {
                return new Command(async () =>
                {
                    try
                    {
                        string filename = $"{_area}_{DateTime.Now.Date.ToString("dd_MM_yyyy")}.txt";
                        string fullPath = Path.Combine(DependencyService.Get<IStorageUtil>().GetDownloadsPath(), filename);
                        string contentToExport = string.Empty;

                        foreach (var item in InventoryCount)
                        {
                            contentToExport += $"{item.Area},{item.Code},{item.Count}\r\n";
                        }
                        File.WriteAllText(fullPath, contentToExport);
                        await Application.Current.MainPage.DisplayAlert("", "Archivo exportado", "Aceptar");
                    }
                    catch (Exception ex)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "Aceptar");
                    }
                });
            }
        }

        public void LoadStoreItems()
        {
            try
            {
                StoreItems.Clear();
                var resourceName = Path.Combine(DependencyService.Get<IStorageUtil>().GetDownloadsPath(), "PDA_ART.txt");
                using (FileStream stream = new FileStream(resourceName, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var result = line.Split(',');
                        StoreItems.Add(new InventoryItem
                        {
                            Code = result[0],
                            Name = result[1]
                        });
                    }
                }
            }
            catch (Exception)
            {
                Task.Run(async () => await Application.Current.MainPage.DisplayAlert("Error", "Catálogo de productos no encontrado", "Aceptar"));
            }
        }

        public async Task LoadInventoryItems()
        {
            InventoryCount.Clear();
            TotalItemsCount = 0;
            TotalInventory = 0;
            var savedItems = await _dbContext.GetItemsAsync();
            savedItems = savedItems.Where(i => i.Area == _area).ToList();
            foreach (var item in savedItems)
            {
                InventoryCount.Add(new InventoryItem
                {
                    Id = item.Id,
                    Name = item.Name,
                    Code = item.Code,
                    Count = item.Count,
                    Area = item.Area,
                });
                TotalInventory += item.Count;
            }

            TotalItemsCount = InventoryCount.Count;
        }
    }
}