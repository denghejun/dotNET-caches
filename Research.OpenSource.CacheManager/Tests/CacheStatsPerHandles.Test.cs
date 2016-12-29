using CacheManager.Core;
using CacheManager.Core.Internal;
using CacheManager.Core.Logging;
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
    internal sealed class CacheStatsPerHandlesTest
    {
        [Test]
        public void CacheStatsShouldShowSuccess()
        {
            var manager = CacheFactory.Build<List<Company>>(settings =>
            {
                settings.WithDictionaryHandle().WithExpiration(ExpirationMode.Absolute, TimeSpan.FromSeconds(1)).EnableStatistics().EnablePerformanceCounters();
                settings.WithSystemRuntimeCacheHandle().EnableStatistics();
                settings.WithUpdateMode(CacheUpdateMode.None);
            });

            // all cache layer hold the ITCompanies.
            manager.Add(Company.CACHE_KEY, Company.ITCompanies);
            manager.Put(Company.CACHE_KEY, Company.MovieCompanies);
            manager.Remove(Company.CACHE_KEY);
            manager.Add(Company.CACHE_KEY, Company.ITCompanies);

            var cachesFinal = manager.Get(Company.CACHE_KEY);
            foreach (var handle in manager.CacheHandles)
            {
                var stats = handle.Stats;
                Console.WriteLine(string.Format(
                        "Items: {0}, Hits: {1}, Miss: {2}, Remove: {3}, ClearRegion: {4}, Clear: {5}, Adds: {6}, Puts: {7}, Gets: {8}",
                            stats.GetStatistic(CacheStatsCounterType.Items),
                            stats.GetStatistic(CacheStatsCounterType.Hits),
                            stats.GetStatistic(CacheStatsCounterType.Misses),
                            stats.GetStatistic(CacheStatsCounterType.RemoveCalls),
                            stats.GetStatistic(CacheStatsCounterType.ClearRegionCalls),
                            stats.GetStatistic(CacheStatsCounterType.ClearCalls),
                            stats.GetStatistic(CacheStatsCounterType.AddCalls),
                            stats.GetStatistic(CacheStatsCounterType.PutCalls),
                            stats.GetStatistic(CacheStatsCounterType.GetCalls)
                        ));
            }
        }
    }
}
