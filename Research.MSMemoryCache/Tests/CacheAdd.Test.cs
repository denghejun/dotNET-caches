using NUnit.Framework;
using Research.MSMemoryCache.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Research.MSMemoryCache.Tests
{
    [TestFixture]
    internal sealed class CacheNormalTest
    {
        [Test]
        public void CacheShouldAddSuccess()
        {
            // 微软建议尽可能使用Default缓存实例，而不是创建任意多的新的MemoryCache实例。
            MemoryCache.Default.Add(Warehouse.CACHE_KEY, Warehouse.Warehouses, DateTimeOffset.Now.AddSeconds(2));
            var warehousesFromCache = MemoryCache.Default.Get(Warehouse.CACHE_KEY) as List<Warehouse>;
            Assert.That(warehousesFromCache, Is.Not.Null);
            Assert.That(warehousesFromCache.Count, Is.EqualTo(2));
        }

        [Test]
        public void CacheAddShouldFailed()
        {
            MemoryCache.Default.Add(Warehouse.CACHE_KEY, Warehouse.Warehouses, MemoryCache.InfiniteAbsoluteExpiration);
            bool result = MemoryCache.Default.Add(Warehouse.CACHE_KEY, Warehouse.Warehouses.Where(o => o.WarehouseNumber == "07").ToList(), MemoryCache.InfiniteAbsoluteExpiration);
            var caches = MemoryCache.Default.Get(Warehouse.CACHE_KEY) as List<Warehouse>;
            Assert.That(result, Is.EqualTo(false));
            Assert.That(caches.Count, Is.EqualTo(2));
        }
    }
}
