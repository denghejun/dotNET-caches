using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Research.MSMemoryCache.Tests
{
    [TestFixture]
    internal sealed class CacheConfigFileTest
    {
        [Test]
        public void CacheConfigFileMonitorShouldSuccess()
        {
            var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "warehouse.txt");
            var contents = MemoryCache.Default.Get("WarehouseConfig");
            if (contents == null)
            {
                var policy = new CacheItemPolicy()
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(30),
                };

                policy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string>() { configFilePath }));
                MemoryCache.Default.Set("WarehouseConfig", File.ReadAllText(configFilePath), policy);
            }

            Thread.Sleep(5000);
            var contentsFromCache = MemoryCache.Default.Get("WarehouseConfig");
            Assert.That(contentsFromCache, Is.Not.Null);
            Assert.That(contentsFromCache, Is.EqualTo("01,02,03"));
        } 
    }
}
