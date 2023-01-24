using InventoryCount.App.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InventoryCount.App.Utils
{
    public class dbInventory
    {
        static SQLiteAsyncConnection Database;

        public static readonly AsyncLazy<dbInventory> Instance = new AsyncLazy<dbInventory>(async () =>
        {
            var instance = new dbInventory();
            CreateTableResult result = await Database.CreateTableAsync<InventoryItem>();
            return instance;
        });

        public dbInventory()
        {
            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        }

        public Task<List<InventoryItem>> GetItemsAsync()
        {
            return Database.Table<InventoryItem>().ToListAsync();
        }

        //public Task<List<InventoryItem>> GetItemsNotDoneAsync()
        //{
        //    // SQL queries are also possible
        //    return Database.QueryAsync<InventoryItem>("SELECT * FROM [InventoryItem] WHERE [Done] = 0");
        //}

        public Task<InventoryItem> GetItemAsync(string code)
        {
            return Database.Table<InventoryItem>().Where(i => i.Code == code).FirstOrDefaultAsync();
        }

        public Task<int> SaveItemAsync(InventoryItem item)
        {
            if (item.Id != 0)
            {
                return Database.UpdateAsync(item);
            }
            else
            {
                return Database.InsertAsync(item);
            }
        }

        public Task<int> DeleteItemAsync(InventoryItem item)
        {
            return Database.DeleteAsync(item);
        }

        public Task<int> DeleteAllAsync()
        {
            return Database.DeleteAllAsync<InventoryItem>();
        }
    }

    public class AsyncLazy<T>
    {
        readonly Lazy<Task<T>> instance;

        public AsyncLazy(Func<T> factory)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        public AsyncLazy(Func<Task<T>> factory)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return instance.Value.GetAwaiter();
        }
    }
}
