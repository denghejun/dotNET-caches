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
    internal sealed class CacheRemoveTest
    {
        [Test]
        public void CacheShouldBeRemovedSuccess()
        {
            MemoryCache.Default.Add(Warehouse.CACHE_KEY, Warehouse.Warehouses, MemoryCache.InfiniteAbsoluteExpiration);
            MemoryCache.Default.Remove(Warehouse.CACHE_KEY);
            var caches = MemoryCache.Default.Get(Warehouse.CACHE_KEY);
            Assert.That(caches, Is.Null);
        }
    }
}
