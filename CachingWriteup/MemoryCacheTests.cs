using Microsoft.Extensions.Caching.Memory;

namespace CachingWriteup;

public class MemoryCacheTests
{
    [Fact]
    public void MemoryCache()
    {
        var cacheKey = Guid.NewGuid().ToString();
        
        var myCache = new MemoryCache(new MemoryCacheOptions());
        myCache.Set(cacheKey, new TestObject()
        {
            Value = 1
        });

        var copy1 = myCache.Get<TestObject>(cacheKey);
        var copy2 = myCache.Get<TestObject>(cacheKey);

        copy1.Value = 1337;

        Assert.Equal(copy1.Value, copy2.Value);
    }
}