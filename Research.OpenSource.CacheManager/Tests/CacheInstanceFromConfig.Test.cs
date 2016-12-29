using CacheManager.Core;
using NUnit.Framework;
using Research.OpenSource.CacheManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Research.OpenSource.CacheManager.Tests
{
    [TestFixture]
    internal sealed class CacheInstanceFromConfig
    {
        [Test]
        public void CreateCacheManagerFromConfigShouldSuccess()
        {
            var cfg = ConfigurationBuilder.LoadConfigurationFile("Caches.config", "ITCompaniesCacheConfig");
            var manager = CacheFactory.FromConfiguration<List<Company>>(cfg);
            manager.Add(Company.CACHE_KEY, Company.ITCompanies);
            var caches = manager.Get(Company.CACHE_KEY);

            Assert.That(caches, Is.Not.Null);
            Assert.That(caches.Count, Is.EqualTo(2));
        }

        [Test]
        public void CreateCacheManagerWithCodeConfigShouldSuccess()
        {
            var cfg = ConfigurationBuilder.BuildConfiguration(settings =>
            {
                settings.WithDictionaryHandle().WithExpiration(ExpirationMode.Absolute, TimeSpan.FromSeconds(2));
            });

            var manager = CacheFactory.FromConfiguration<List<Company>>(cfg);
            manager.Add(Company.CACHE_KEY, Company.ITCompanies);

            Thread.Sleep(3000);

            var caches = manager.Get(Company.CACHE_KEY);
            Assert.That(caches, Is.Null);
        }
    }
}
