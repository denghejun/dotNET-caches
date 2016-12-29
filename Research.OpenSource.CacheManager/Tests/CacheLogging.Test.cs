using CacheManager.Core;
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
    internal class CacheLoggingTest
    {
        [Test]
        public void CacheConsoleLogShouldShowSuccess()
        {
            var manager = CacheFactory.Build<List<Company>>(settings =>
            {
                settings.WithDictionaryHandle().WithExpiration(ExpirationMode.Absolute, TimeSpan.FromSeconds(1)).EnableStatistics().EnablePerformanceCounters();
                settings.WithSystemRuntimeCacheHandle().EnableStatistics();
                settings.WithUpdateMode(CacheUpdateMode.None);
                settings.WithLogging(typeof(CustomerLogFactory));
            });

            // all cache layer hold the ITCompanies.
            manager.Add(Company.CACHE_KEY, Company.ITCompanies);
            manager.Put(Company.CACHE_KEY, Company.MovieCompanies);
            manager.Remove(Company.CACHE_KEY);
            manager.Add(Company.CACHE_KEY, Company.ITCompanies);

            var caches = manager.Get(Company.CACHE_KEY);
            Assert.That(caches, Is.Not.Null);
        }
    }

    public class CustomerLogFactory : ILoggerFactory
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new ConsoleLogger();
        }

        public ILogger CreateLogger<T>(T instance)
        {
            return new ConsoleLogger();
        }
    }

    internal class ConsoleLogger : ILogger
    {
        public IDisposable BeginScope(object state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log(LogLevel logLevel, int eventId, object message, Exception exception)
        {
            //Console.WriteLine("------------ CONSOLE LOG --------------");
            Console.WriteLine("MSG: " + message?.ToString());
        }
    }
}
