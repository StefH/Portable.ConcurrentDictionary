# Portable.ConcurrentDictionary

## Info
| | |
| --- | --- |
| **Build AppVeyor** | [![Build status](https://ci.appveyor.com/api/projects/status/4ujh5c5qvabw5i5p?svg=true)](https://ci.appveyor.com/project/StefH/portable-concurrentdictionary) |
| **Portable.ConcurrentDictionary** | [![NuGet Badge Portable.ConcurrentDictionary](https://buildstats.info/nuget/Portable.ConcurrentDictionary)](https://www.nuget.org/packages/Portable.ConcurrentDictionary) |

## Framework support

This is a backport from System.Collections.Concurrent.ConcurrentDictionary defined in .NET Core and this library helps supporting frameworks:
 - .NET Standard 1.0
 - .NET 2.0
 - .NET 3.5
 - Silverlight 5.0
 - Windows Phone Silverlight 8.0
 - Windows Phone Silverlight 8.1
 - Windows Phone 8.1
 - Windows 8

## ConcurrentDictionary

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