using CacheManager.Core;
using CacheManager.Core.Configuration;
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
    internal sealed class CacheAddOrUpdateTest
    {
        [Test]
        public void CacheShouldAddSuccessWhenNotExisting()
        {
            var manager = CacheFactory.Build<List<Warehouse>>(settings =>
            {
                settings.WithDictionaryHandle();
            });

            manager.AddOrUpdate(Warehouse.CACHE_KEY, Warehouse.Warehouses, o => o.Union(Warehouse.Warehouses).ToList());
            var caches = manager.Get(Warehouse.CACHE_KEY);

            Assert.That(caches.Count, Is.EqualTo(2));
        }

        [Test]
        public void CacheShouldUpdateSuccessWhenCacheExiting()
        {
            var manager = CacheFactory.Build<List<Company>>(settings =>
            {
                settings.WithDictionaryHandle();
            });

            manager.Add(Company.CACHE_KEY, Company.MovieCompanies);
            manager.AddOrUpdate(Company.CACHE_KEY, Company.ITCompanies, o => Company.ITCompanies);
            
            var caches = manager.Get(Company.CACHE_KEY);
            Assert.That(caches, Is.Not.Null);
            Assert.That(caches.Count, Is.EqualTo(2));
            Assert.That(caches[0].CompanyType, Is.EqualTo(CompanyType.ITCompany));
        }
    }
}
