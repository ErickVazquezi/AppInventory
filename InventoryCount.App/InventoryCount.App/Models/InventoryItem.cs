using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryCount.App.Models
{
    public class InventoryItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public int Area { get; set; }
    }
}
