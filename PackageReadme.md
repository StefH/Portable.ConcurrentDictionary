## Portable.ConcurrentDictionary
ConcurrentDictionary is quite usuful dictionary with a thread-safe feature.

### Examples

```csharp
// New instance. (from http://www.dotnetperls.com/concurrentdictionary)
var con = new ConcurrentDictionary<string, int>();
con.TryAdd("cat", 1);
con.TryAdd("dog", 2);

// Try to update if value is 4 (this fails).
con.TryUpdate("cat", 200, 4);

// Try to update if value is 1 (this works).
con.TryUpdate("cat", 100, 1);

// Write new value.
Console.WriteLine(con["cat"]);
```

This project is based on https://github.com/SaladLab/NetLegacySupport


### Sponsors

[Entity Framework Extensions](https://entityframework-extensions.net/?utm_source=StefH) and [Dapper Plus](https://dapper-plus.net/?utm_source=StefH) are major sponsors and proud to contribute to the development of **Portable.ConcurrentDictionary**.

[![Entity Framework Extensions](https://raw.githubusercontent.com/StefH/resources/main/sponsor/entity-framework-extensions-sponsor.png)](https://entityframework-extensions.net/bulk-insert?utm_source=StefH)

[![Dapper Plus](https://raw.githubusercontent.com/StefH/resources/main/sponsor/dapper-plus-sponsor.png)](https://dapper-plus.net/bulk-insert?utm_source=StefH)