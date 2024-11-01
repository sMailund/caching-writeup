using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CachingWriteup;

public class DistributedCacheTests
{
    // endre til true hvis du har satt opp lokal redis på localhost:6379
    private const bool UseRedis = false;

    [Fact]
    public void DistributedMemoryCacheTest()
    {
        var cacheKey = Guid.NewGuid().ToString();

        var inMemoryDistributedCacheOptions = Options.Create(new MemoryDistributedCacheOptions());
        var redisDistributedCacheOptions = new RedisCacheOptions
        {
            Configuration = "localhost:6379",
        };

        // Hvis du ikke har skrudd på for å gå mot redis bruker vi MemoryDistributedCache implementasjonen istedenfor.
        // Vi bruker samme interface, så operasjonene som er mulige er de samme uavhengig av hvilken implementasjon vi bruker.
        IDistributedCache myCache =
            UseRedis
                ? new RedisCache(redisDistributedCacheOptions)
                : new MemoryDistributedCache(inMemoryDistributedCacheOptions);

        var testObject = new TestObject
        {
            Value = 1
        };

        // IDistributedCache har bare støtte for å hente og lese verdier fra cache som byte-array.
        // Det finnes ingen metoder som henter elelr setter selve objektet på samme måte som i MemoryCache.
        // Som eksempel kan du kommentere ut denne linjen og se at det er en kompileringsfeil:
        // var notAllowed = myCache.Get<TestObject>(cacheKey);

        var jsonData = JsonConvert.SerializeObject(testObject);
        myCache.SetString(cacheKey, jsonData);

        // Siden vi bare kan hente ut som byte-array, må vi deserialisere til objektet her.
        // Verken byte-arrayen eller det deserialiserte objektet har en minnereferanse representasjonen i cachet.
        var get1 = myCache.GetString(cacheKey)!;
        var copy1 = JsonConvert.DeserializeObject<TestObject>(get1);

        // Her henter vi ut samme entry fra cachet.
        // Siden kopiene ikke deler minnereferanse til det originale objektet behandles de som to urelaterte objekter
        var get2 = myCache.GetString(cacheKey)!;
        var copy2 = JsonConvert.DeserializeObject<TestObject>(get2);

        // her endrer vi verdien i den første kopien
        copy1!.Value = 1337;

        // og her kan vi sjekke at verdiene ikke er like etter vi endra i den første kopien
        Assert.NotEqual(copy1.Value, copy2!.Value);
        
        // for å endre verdien i cachet kan vi skrive tilbake til redis for å oppdatere verdien:
        var updatedJson = JsonConvert.SerializeObject(copy1);
        myCache.SetString(cacheKey, updatedJson);
        
        // her henter vi ut enda en kopi etter at vi har skrevet tilbake endringene i copy1
        var getAfterChange = myCache.GetString(cacheKey)!;
        var copyAfterChange = JsonConvert.DeserializeObject<TestObject>(getAfterChange);
        
        // copy2 har ikke blitt oppdatert med nye verdier siden det ble hentet før endringene
        Assert.NotEqual(copyAfterChange!.Value, copy2.Value);
        
        // copyAfterChange ble hentet ut etter vi skrev tilbake endringene i copy1, så det har fått med seg oppdateringene
        Assert.Equal(copyAfterChange.Value, copy1.Value);
    }
    
}