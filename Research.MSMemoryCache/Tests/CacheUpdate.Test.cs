using NUnit.Framework;
using Research.MSMemoryCache.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Research.MSMemoryCache.Tests
{
    [TestFixture]
    internal sealed class CacheUpdateTest
    {
        [Test]
        public void CacheUpdateShouldSuccess()
        {
            // nerver expired.
            MemoryCache.Default.Add(Warehouse.CACHE_KEY, Warehouse.Warehouses, MemoryCache.InfiniteAbsoluteExpiration);
            var specialWarehouse = Warehouse.Warehouses.Where(o => o.WarehouseNumber == "07").ToList();
            MemoryCache.Default.Set(Warehouse.CACHE_KEY, specialWarehouse, MemoryCache.InfiniteAbsoluteExpiration);
            var caches = MemoryCache.Default.Get(Warehouse.CACHE_KEY) as List<Warehouse>;
            Assert.That(caches, Is.Not.Null);
            Assert.That(caches.Count, Is.EqualTo(1));
            Assert.That(caches[0].WarehouseNumber == "07");
        }
    }
}
