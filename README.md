## Microsoft MemoryCache

* `MemoryCache`是.NET唯一实现了`ObjectCache`的类，可以使用它来在内存中缓存任意类型对象。
* `MemoryCache.Default`是默认的`MemoryCache`实例，.NET建议使用它而不是创建任意多个缓存对象。

###### Add Cache
`Add`方法会在已存在相同`Key`的缓存时返回`false`，所以不会重复添加。

```
MemoryCache.Default.Add(Warehouse.CACHE_KEY, Warehouse.Warehouses, DateTimeOffset.Now.AddSeconds(2));
var warehousesFromCache = MemoryCache.Default.Get(Warehouse.CACHE_KEY) as List<Warehouse>;
Assert.That(warehousesFromCache, Is.Not.Null);
Assert.That(warehousesFromCache.Count, Is.EqualTo(2));
```


###### Add Or Get Existing
返回新添加或已存在相同`Key`的缓存Value。
```
MemoryCache.Default.Add(Warehouse.CACHE_KEY, Warehouse.Warehouses, MemoryCache.InfiniteAbsoluteExpiration);
var caches = MemoryCache.Default.AddOrGetExisting(Warehouse.CACHE_KEY, Warehouse.Warehouses.Where(o => o.WarehouseNumber == "07").ToList(), MemoryCache.InfiniteAbsoluteExpiration) as List<Warehouse>;
Assert.That(caches.Count, Is.Not.EqualTo(1));
Assert.That(caches.Count, Is.EqualTo(2));
```

###### Update Cache
更新（替换）缓存。
```
MemoryCache.Default.Add(Warehouse.CACHE_KEY, Warehouse.Warehouses, MemoryCache.InfiniteAbsoluteExpiration);
var specialWarehouse = Warehouse.Warehouses.Where(o => o.WarehouseNumber == "07").ToList();
MemoryCache.Default.Set(Warehouse.CACHE_KEY, specialWarehouse, MemoryCache.InfiniteAbsoluteExpiration);
var caches = MemoryCache.Default.Get(Warehouse.CACHE_KEY) as List<Warehouse>;
Assert.That(caches, Is.Not.Null);
Assert.That(caches.Count, Is.EqualTo(1));
Assert.That(caches[0].WarehouseNumber == "07");
```

###### Cache Expire
有两种缓存过期策略：
* AbsoluteExpiration：绝对时间过期。在指定的时间间隔后过期，缓存移除、丢失。
* SlidingExpiration：滑动时间过期。在指定的时间间隔后若未命中过该缓存，则移除、丢失；否则，则顺延指定的时间间隔，缓存不移除。

```
MemoryCache.Default.Add(Warehouse.CACHE_KEY+"1", Warehouse.Warehouses, DateTimeOffset.Now.AddSeconds(2));
Thread.Sleep(2000);
var warehousesFromCache = MemoryCache.Default.Get(Warehouse.CACHE_KEY+"1") as List<Warehouse>;
Assert.That(warehousesFromCache, Is.Null);
```

###### Cache Refresh
当Cache丢失前，MemoryCache会回掉一个Callback委托进行通知，此时，为了让应用程序没有该缓存真空期，我们应该立即刷新缓存，保持最新的同时防止客户端出现缓存未命中（缓存丢失）的现象发生。
```
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
```

###### Cache Config File
在需要缓存文件内容时适用；且当文件内容更改时自动刷新缓存。
```
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
```
## Open Source CacheManager
`CacheManager`是`GitHub`上一个开源的专注于`.NET Cache`领域项目，其有许多新的思想和概念是`.NET MemoryCache`所不具备的；相对于`MemoryCache`，其有以下特有`feature`：
* 代码实现、写法上更符合现如今的可读性、连续性的友好风格
* 缓存抽象出`Layer`的概念，同一份缓存数据可以同时缓存到多个`Layer`（CacheManager称之为'Handle'）上，有利于防止某`Layer`缓存意外丢失，降低客户端换成未命中的概率，其中`DictionaryHandle`性能最好
* 每层可单独定义过期策略，并指定层与层之间当发现缓存不一致时（有的丢失了，有的还在）的更新策略
* 支持通过配置文件(.config)形式定义缓存策略（过期策略、缓存`Handle`等），就可以不修改代码来随时更改
* 有针对每个`Layer`缓存的Add、Remove、Update、Hit统计信息输出
* 有提供Logging接口，以记录缓存在Add、Remove、Update、Hit时的详细信息输入
* 有提供分布式缓存不一致的解决方案（ClientA、ClientB共享同一份缓存，同时各自缓存一份In-Process缓存在本地，当ClientA更新了本地缓存时，通过消息的方式通知ClientB更新本地缓存）

###### Add Cache
添加缓存。已存在时返回`false`
```
var cacheManager = CacheFactory.Build<List<Warehouse>>(setting =>
{
    setting.WithDictionaryHandle();
    setting.WithSystemRuntimeCacheHandle();
});

cacheManager.Add(Warehouse.CACHE_KEY, Warehouse.Warehouses);
var caches = cacheManager.Get(Warehouse.CACHE_KEY);
Assert.That(caches.Count, Is.EqualTo(2));
Assert.That(cacheManager.CacheHandles.Count, Is.EqualTo(2));
```


###### Add Or Get Existing
返回新添加的或已存在相同`Key`的缓存。
```
var cacheManager = CacheFactory.Build<List<Warehouse>>(settings =>
{
    settings.WithDictionaryHandle();
});

var caches = cacheManager.GetOrAdd(Warehouse.CACHE_KEY, Warehouse.Warehouses);
Assert.That(caches.Count, Is.EqualTo(2));
```

###### Add Or Update Existing
不存在就添加，存在就更新。
```
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
```

###### Update Cache
更新缓存。
```
var manager = CacheFactory.Build(settings =>
{
    settings.WithDictionaryHandle();
});

manager.Add(Company.CACHE_KEY, Company.MovieCompanies);
manager.Update(Company.CACHE_KEY, o => Company.ITCompanies.Union(o as List<Company>).ToList());
var caches = manager.Get<List<Company>>(Company.CACHE_KEY);

Assert.That(caches.Count, Is.EqualTo(4));
```

###### Create Cache Instance From Config
从配置文件加载缓存策略等信息。
* Caches.config

  ```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <section name="cacheManager" type="CacheManager.Core.Configuration.CacheManagerSection, CacheManager.Core" />
  </configSections>

  <cacheManager>
    <managers>
      <cache  name="ITCompaniesCacheConfig" updateMode="Up" enableStatistics="false" enablePerformanceCounters="false">
        <handle name="handleName" ref="systemRuntimeHandle" expirationMode="Absolute" timeout="50s"/>
      </cache>
      <cache  name="MoviesCompaniesCacheConfig" updateMode="Up" enableStatistics="false" enablePerformanceCounters="false">
        <handle name="handleName" ref="systemRuntimeHandle" expirationMode="Absolute" timeout="50s"/>
      </cache>
    </managers>
    <cacheHandles>
      <handleDef  id="systemRuntimeHandle" type="CacheManager.SystemRuntimeCaching.MemoryCacheHandle`1, CacheManager.SystemRuntimeCaching"
          defaultExpirationMode="Sliding" defaultTimeout="5m"/>
    </cacheHandles>
  </cacheManager>
</configuration>
  ```
* Create Cache Instance From Config

```
var cfg = ConfigurationBuilder.LoadConfigurationFile("Caches.config", "ITCompaniesCacheConfig");
var manager = CacheFactory.FromConfiguration<List<Company>>(cfg);
manager.Add(Company.CACHE_KEY, Company.ITCompanies);
var caches = manager.Get(Company.CACHE_KEY);

Assert.That(caches, Is.Not.Null);
Assert.That(caches.Count, Is.EqualTo(2));
```

###### Cache With Multiple Layers
将同一份数据缓存到多个`Layer`，会按`Layer`（Handle）的添加顺序去命中缓存（返回第一个命中的）。
```
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
```

###### Cache Expire
缓存过期是针对每个`Layer`的，可以用连续调用的方式来初始化一个带有过期策略的`CacheManager`。

```
var manager = CacheFactory.Build<List<Company>>(settings =>
{
 settings.WithDictionaryHandle().WithExpiration(ExpirationMode.Absolute, TimeSpan.FromSeconds(1));
 settings.WithSystemRuntimeCacheHandle();
 settings.WithUpdateMode(CacheUpdateMode.None);
});
```
注意这里的`WithUpdateMode`,`CacheUpdateMode`有三种值，分别代表：
* CacheUpdateMode.None: 无论缓存首先被命中在哪一个的`Layer`，各个`Layer`存储的缓存不做任何更新即使它们彼此不一样；
* CacheUpdateMode.Full: 无论缓存首先被命中在哪一个的`Layer`，各个`Layer`存储的缓存将被更新一致当它们彼此不一样时；
* CacheUpdateMode.UP: 更新缓存首先被命中的那个`Layer`添加顺序之上的所有`Layer`的缓存以保持一致。

###### Cache Statistics
`Cache`在各种`Operation`下的次数统计信息输出。
```
var manager = CacheFactory.Build<List<Company>>(settings =>
{
    settings.WithDictionaryHandle().WithExpiration(ExpirationMode.Absolute,   	 	    TimeSpan.FromSeconds(1)).EnableStatistics().EnablePerformanceCounters();    
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
```

###### Cache Logging
支持`Cache`在各种`Operation`下的日志记录。
```
var manager = CacheFactory.Build<List<Company>>(settings =>
{
    settings.WithDictionaryHandle().WithExpiration(ExpirationMode.Absolute,  TimeSpan.FromSeconds(1)).EnableStatistics().EnablePerformanceCounters();
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
```

OutPut Like Below:
```
CacheManager.Core.BaseCacheManager<object>: Trace: Add or update: key .
CacheManager.Core.BaseCacheManager<object>: Trace: Add: key 
CacheManager.Core.BaseCacheManager<object>: Trace: Add: key  to handle redis FAILED. Evicting items from other handles.
CacheManager.Core.BaseCacheManager<object>: Trace: Evict from other handles: key : excluding handle 1.
CacheManager.Core.BaseCacheManager<object>: Trace: Evict from handle: key : on handle default.
CacheManager.Core.BaseCacheManager<object>: Trace: Add or update: key : add failed, trying to update...
CacheManager.Core.BaseCacheManager<object>: Trace: Update: key .
CacheManager.Core.BaseCacheManager<object>: Trace: Update: key : tried on handle redis: result: Success.
CacheManager.Core.BaseCacheManager<object>: Trace: Evict from handles above: key : above handle 1.
CacheManager.Core.BaseCacheManager<object>: Trace: Evict from handle: key : on handle default.
CacheManager.Core.BaseCacheManager<object>: Trace: Add to handles below: key : below handle 1.
CacheManager.Core.BaseCacheManager<object>: Trace: Add or update: key : successfully updated.
```