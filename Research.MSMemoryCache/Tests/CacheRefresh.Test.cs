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
    internal sealed class CacheRefreshTest
    {
        [Test]
        public void CacheShouldRefreshSuccessAfterExpired()
        {
            MemoryCache.Default.Set(Warehouse.CACHE_KEY, Warehouse.Warehouses, new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(2),
                UpdateCallback = this.CacheRefreshCallback
            });

            Thread.Sleep(5000);
            var warehousesFromCache = MemoryCache.Default.Get(Warehouse.CACHE_KEY) as List<Warehouse>;
            Assert.That(warehousesFromCache, Is.Not.Null);
        }

        private void CacheRefreshCallback(CacheEntryUpdateArguments args)
        {
            var cacheItem = MemoryCache.Default.GetCacheItem(args.Key);
            var cacheObj = cacheItem.Value;

            cacheItem.Value = cacheObj;
            args.UpdatedCacheItem = cacheItem;
            var policy = new CacheItemPolicy
            {
                UpdateCallback = new CacheEntryUpdateCallback(CacheRefreshCallback),
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(2)
            };

            args.UpdatedCacheItemPolicy = policy;
        }
    }
}
