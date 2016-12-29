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
    internal sealed class CacheExpiredTest
    {
        [Test]
        public void CacheShouldExpireSuccess()
        {
            MemoryCache.Default.Add(Warehouse.CACHE_KEY+"1", Warehouse.Warehouses, DateTimeOffset.Now.AddSeconds(2));
            Thread.Sleep(3000);
            var warehousesFromCache = MemoryCache.Default.Get(Warehouse.CACHE_KEY+"1") as List<Warehouse>;
            Assert.That(warehousesFromCache, Is.Null);
        }
    }
}
