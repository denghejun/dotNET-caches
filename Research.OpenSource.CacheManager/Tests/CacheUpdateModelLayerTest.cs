using CacheManager.Core;
using CacheManager.Core.Internal;
using NUnit.Framework;
using Research.OpenSource.CacheManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CacheManager.Core.Logging;
using System.Threading;

namespace Research.OpenSource.CacheManager.Tests
{
    [TestFixture]
    internal sealed class CacheUpdateModelLayerTest
    {
        [Test]
        public void CacheLayersAllShouldNotUpdate()
        {
            var manager = CacheFactory.Build<List<Company>>(settings =>
             {
                 settings.WithDictionaryHandle().WithExpiration(ExpirationMode.Absolute, TimeSpan.FromSeconds(1));
                 settings.WithSystemRuntimeCacheHandle();
                 settings.WithUpdateMode(CacheUpdateMode.None);
             });

            // all cache layer hold the ITCompanies.
            manager.Add(Company.CACHE_KEY, Company.ITCompanies);

            Thread.Sleep(1000);

            var caches = manager.Get(Company.CACHE_KEY); // ONLY When GET: If find some layer was difference from others, the "Update" strategy will be executed.

            Assert.That(caches, Is.Not.Null);
            Assert.That(caches.Count, Is.EqualTo(2));

            Assert.That(manager.CacheHandles.Count, Is.EqualTo(2));
            Assert.That(manager.CacheHandles.First().Count, Is.EqualTo(0));
            Assert.That(manager.CacheHandles.Last().Count, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void CacheLayerAboveShouldSyncUpdateSuccess()
        {
            var manager = CacheFactory.Build<List<Company>>(settings =>
            {
                settings.WithUpdateMode(CacheUpdateMode.Up);
                settings.WithDictionaryHandle().WithExpiration(ExpirationMode.Absolute, TimeSpan.FromSeconds(1));
                settings.WithSystemRuntimeCacheHandle();
            });

            // all cache layer hold the ITCompanies.
            manager.Add(Company.CACHE_KEY, Company.ITCompanies);

            Thread.Sleep(1000);

            var caches = manager.Get(Company.CACHE_KEY); // ONLY When GET: If find some layer was difference from others, the "Update" strategy will be executed.

            Assert.That(caches, Is.Not.Null);
            Assert.That(caches.Count, Is.EqualTo(2));

            Assert.That(manager.CacheHandles.Count, Is.EqualTo(2));
            Assert.That(manager.CacheHandles.ElementAt(0).Count, Is.EqualTo(1));
            Assert.That(manager.CacheHandles.ElementAt(1).Count, Is.GreaterThanOrEqualTo(1));
        }
    }
}
