# Portable.ConcurrentDictionary

[![Build status](https://ci.appveyor.com/api/projects/status/4ujh5c5qvabw5i5p?svg=true)](https://ci.appveyor.com/project/StefH/Portable.ConcurrentDictionary)

Portable, Silverlight 5.0 and Windows Phone Silverlight 8.0 & 8.1 implementation of the ConcurrentDictionary

This is a backport from System.Collections.Concurrent.ConcurrentDictionary defined in .NET Core and this library helps supporting frameworks like:
 - Silverlight 5.0
 - Windows Phone Silverlight 8.0 
 - Windows Phone Silverlight 8.1

## ConcurrentDictionary

ConcurrentDictionary is quite usuful dictionary with a thread-safe feature.

#### Where can I get it?

```
PM> Install-Package Portable.ConcurrentDictionary
```

#### Code that you can write.

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