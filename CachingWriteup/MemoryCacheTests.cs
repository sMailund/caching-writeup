using Microsoft.Extensions.Caching.Memory;

namespace CachingWriteup;

public class MemoryCacheTests
{
    [Fact]
    public void MemoryCache()
    {
        var cacheKey = Guid.NewGuid().ToString();
        
        // siden vi bare jobber i lokalt minne lager vi cache med IMemoryCache
        IMemoryCache myCache = new MemoryCache(new MemoryCacheOptions());
        
        var testObject = new TestObject()
        {
            Value = 1
        };
        
        myCache.Set(cacheKey, testObject);

        // her henter vi ut objektet vi satte inn 2 ganger, de deler referanse til samme objekt i minne
        var copy1 = myCache.Get<TestObject>(cacheKey);
        var copy2 = myCache.Get<TestObject>(cacheKey);

        // så når vi endrer via den ene pekeren...
        copy1.Value = 1337;

        // ...har vi endra på objektet som den andre pekeren også peker på
        Assert.Equal(copy1.Value, copy2.Value);
    }
}