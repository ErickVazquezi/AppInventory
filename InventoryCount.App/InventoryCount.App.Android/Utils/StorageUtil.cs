using Android.OS;
using InventoryCount.App.Utils;
using System.IO;

namespace InventoryCount.App.Droid.Utils
{
    public  class StorageUtil : IStorageUtil
    {
        public string GetDownloadsPath()
        {
            return Path.Combine(Environment.ExternalStorageDirectory.AbsolutePath, Environment.DirectoryDownloads);
        }
    }
}