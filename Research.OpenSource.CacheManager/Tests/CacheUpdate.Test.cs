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
    internal sealed class CacheUpdateTest
    {
        [Test]
        public void CacheUpdateShouldSuccess()
        {
            var manager = CacheFactory.Build(settings =>
            {
                settings.WithDictionaryHandle();
            });

            manager.Add(Company.CACHE_KEY, Company.MovieCompanies);
            manager.Update(Company.CACHE_KEY, o => Company.ITCompanies.Union(o as List<Company>).ToList());
            var caches = manager.Get<List<Company>>(Company.CACHE_KEY);

            Assert.That(caches.Count, Is.EqualTo(4));
        }

        [Test]
        public void CacheTryUpdateShouldSuccess()
        {
            var manager = CacheFactory.Build(settings =>
            {
                settings.WithDictionaryHandle();
            });

            manager.Add(Company.CACHE_KEY, Company.MovieCompanies);
            object caches = null;
            var result = manager.TryUpdate(Company.CACHE_KEY, o => Company.ITCompanies.Union(o as List<Company>).ToList(), out caches);

            Assert.That(result, Is.EqualTo(true));
            Assert.That((caches as List<Company>).Count, Is.EqualTo(4));
        }
    }
}
