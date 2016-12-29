using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.OpenSource.CacheManager.Models
{
    public class Warehouse
    {
        public static string CACHE_KEY = "Warehouses";
        public static List<Warehouse> Warehouses
        {
            get
            {
                return new List<Warehouse>()
                {
                    new Warehouse() { WarehouseNumber = "07",Address = "wh07 addr." },
                    new Warehouse() { WarehouseNumber = "08",Address = "wh08 addr." }
                };
            }
        }

        public string WarehouseNumber { get; set; }
        public string Address { get; set; }
    }
}
