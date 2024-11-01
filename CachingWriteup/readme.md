# `IMemoryCache` vs `IDistributedCache`

I sammenheng med en teknisk diskusjon var hoveduenigheten om hvordan cachede objekter håndteres i dotnet
Etter litt graving viser det seg at begge perspektivene er riktige avhengig av kontekst.

## Blir endringer i et cachet objekt reflektert hos alle kopier
Spørsmålet handlet om hva som skjer om man henter samme entry fra cache 2 ganger og endrer den ene kopien.
- blir begge kopiene oppdatert, eller
- blir tilstanden i hver kopi håndtert separat

Dette kan reformuleres som:
- gir cachet en minnereferanse til det samme objektet, eller
- gir cachet en grunn kopi med samme tilstand som objektet i cache

evt som pseudocode:
``` 
var obj1 = cache["my-key"]
var obj2 = cache["my-key"]

obj1.value = 1
obj2.value = 2

obj1.value == obj2.value // true eller false?
```

# svaret: det kommer an på
## forskjellen på `IMemoryCache` og `IDistributedCache`
[Det er to forskjellige måter å cache på i dotnet.](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-8.0).

### `IMemoryCache`
[IMemoryCache](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.memory.imemorycache?view=net-8.0) 
representerer en lokal in-memory cache. Her trenger ikke verdiene å serialiseres, siden dotnet runtimen kan referere direkte til sitt eget minne.
Dette interfacet har metoder for å 
[sette](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.memory.cacheextensions.set?view=net-8.0#microsoft-extensions-caching-memory-cacheextensions-set-1(microsoft-extensions-caching-memory-imemorycache-system-object-0-microsoft-extensions-caching-memory-memorycacheentryoptions))
og 
[gette](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.memory.cacheextensions.get?view=net-8.0#microsoft-extensions-caching-memory-cacheextensions-get-1(microsoft-extensions-caching-memory-imemorycache-system-object))
referanser til objekter i minne uten serialisering.

For at dette skal være mulig må cachet ligge i et minneområdet dotnet runtimen har tilgang på, 
så det finnes ikke noe implementasjon av dette biblioteket for redis.

### `IDistributedCache`
[IDistributedCache](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache?view=net-8.0) 
representerer et cache som *kan* være distribuert, 
dvs at verdiene kan måtte hentes utenfor kontrollsfæren til programmet (f.eks. over nettverket).
På grunn av dette kan man ikke returnere en minnreferanse, og 
[alle tilgjengelige metoder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache?view=net-8.0#methods)
bruker byte-arrays eller strings for å overføre verdier fra og til cachet.
Her finnes det implementasjoner for både in-memory og redis (blant annet).

Når man leser en verdi fra cachet får du bare en kopi av verdien som ligger der, 
og alle endringer på verdien som ble hentet fra cachet blir ikke oppdatert i cachet uten at man aktivt skriver det tilbake.