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
    internal sealed class CacheAddOrGetExisting
    {
        [Test]
        public void CacheShouldBeAddWhenNotExisting()
        {
            MemoryCache.Default.AddOrGetExisting(Warehouse.CACHE_KEY, Warehouse.Warehouses, MemoryCache.InfiniteAbsoluteExpiration);
            var caches = MemoryCache.Default.GetCacheItem(Warehouse.CACHE_KEY);
            Assert.That(caches.Key, Is.EqualTo(Warehouse.CACHE_KEY));
            Assert.That((caches.Value as List<Warehouse>).Count, Is.EqualTo(2));
        }

        [Test]
        public void CacheShouldBeGetExisting()
        {
            MemoryCache.Default.Add(Warehouse.CACHE_KEY, Warehouse.Warehouses, MemoryCache.InfiniteAbsoluteExpiration);
            var caches = MemoryCache.Default.AddOrGetExisting(Warehouse.CACHE_KEY, Warehouse.Warehouses.Where(o => o.WarehouseNumber == "07").ToList(), MemoryCache.InfiniteAbsoluteExpiration) as List<Warehouse>;
            Assert.That(caches.Count, Is.Not.EqualTo(1));
            Assert.That(caches.Count, Is.EqualTo(2));
        }
    }
}
