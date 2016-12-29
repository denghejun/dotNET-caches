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
    internal sealed class CacheAddTest
    {
        [Test]
        public void CacheShouldAddSuccessWithDictionaryHandle()
        {
            var cacheManager = CacheFactory.Build<List<Warehouse>>(setting =>
            {
                setting.WithDictionaryHandle();
            });

            cacheManager.Add(Warehouse.CACHE_KEY, Warehouse.Warehouses);
            var caches = cacheManager.Get(Warehouse.CACHE_KEY);
            Assert.That(caches.Count, Is.EqualTo(2));
        }

        [Test]
        public void CacheShouldAddSucessWithBothDictionaryHandlerAndRuntimeCacheHandle()
        {
            var cacheManager = CacheFactory.Build<List<Warehouse>>(setting =>
            {
                setting.WithDictionaryHandle();
                setting.WithSystemRuntimeCacheHandle();
            });

            cacheManager.Add(Warehouse.CACHE_KEY, Warehouse.Warehouses);
            var caches = cacheManager.Get(Warehouse.CACHE_KEY);
            Assert.That(caches.Count, Is.EqualTo(2));
            Assert.That(cacheManager.CacheHandles.Count, Is.EqualTo(2));
        }

        [Test]
        public void CacheWarehouseAndCompanyShouldAddToSameCacheManagerSuccess()
        {
            var cacheManager = CacheFactory.Build(settings =>
            {
                settings.WithDictionaryHandle();
            });

            cacheManager.Add(Warehouse.CACHE_KEY, Warehouse.Warehouses);
            cacheManager.Add(Company.CACHE_KEY, Company.MovieCompanies);

            var cacheWarehouses = cacheManager.Get<List<Warehouse>>(Warehouse.CACHE_KEY);
            var cacheCompanies = cacheManager.Get<List<Company>>(Company.CACHE_KEY);

            Assert.That(cacheWarehouses.Count, Is.EqualTo(2));
            Assert.That(cacheWarehouses[0].WarehouseNumber, Is.EqualTo("07"));
            Assert.That(cacheCompanies.Count, Is.EqualTo(2));
            Assert.That(cacheCompanies[0].CompanyName, Is.EqualTo("寰宇国际"));
        }

        [Test]
        public void CacheShouldAddFailed()
        {
            var cacheManager = CacheFactory.Build(settings =>
            {
                settings.WithDictionaryHandle();
            });

            cacheManager.Add(Company.CACHE_KEY, Company.MovieCompanies);
            var result = cacheManager.Add(Company.CACHE_KEY, Company.ITCompanies);
            var caches = cacheManager.Get<List<Company>>(Company.CACHE_KEY);

            Assert.That(result, Is.EqualTo(false));
            Assert.That(caches[0].CompanyType, Is.EqualTo(CompanyType.MovieCompany));
        }
    }
}
