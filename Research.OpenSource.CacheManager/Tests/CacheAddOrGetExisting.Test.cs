using CacheManager.Core;
using NUnit.Framework;
using Research.OpenSource.CacheManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.OpenSource.CacheManager.Tests
{
    [TestFixture]
    internal sealed class CacheAddOrGetExistingTest
    {
        [Test]
        public void CacheShouldAddSuccessWhenNotExisting()
        {
            var cacheManager = CacheFactory.Build<List<Warehouse>>(settings =>
            {
                settings.WithDictionaryHandle();
            });

            var caches = cacheManager.GetOrAdd(Warehouse.CACHE_KEY, Warehouse.Warehouses);
            Assert.That(caches.Count, Is.EqualTo(2));
        }

        [Test]
        public void CacheShouldTryToAddSucess()
        {
            var cacheManager = CacheFactory.Build<List<Warehouse>>(settings =>
            {
                settings.WithDictionaryHandle();
            });

            List<Warehouse> caches = null;
            var result = cacheManager.TryGetOrAdd(Warehouse.CACHE_KEY, key => Warehouse.Warehouses, out caches);
            Assert.That(result, Is.EqualTo(true));
            Assert.That(caches.Count, Is.EqualTo(2));
        }
    }
}
