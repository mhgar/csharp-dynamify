# csharp-dynamify
Curry any function to `Func<object[] object>`

## What is this?
This library allows you to create curry functions so that they can be used like `MethodInfo.Invoke()`, except calls become exceptionally faster at the expense of upfront cost for caching.

## How is it used?
You can dynamify any delegate (both Actions or Funcs), for example.

```csharp
Func<string, string, string> concat = (a, b) => a + b;
var dynamicConcat = Dynamify.Make(concat); // Concat can be a member method if you pass it with Func<string, string, string> in front.

var args = new object[] { "Hello", "World" };
var result = dynamicConcat(args) as string; // == "HelloWorld";
```

You can also use `AutoDelegate` to 'steal' from methods from classes at runtime.

```csharp
// Somewhere in a class
public static string Concat(string a, string b) => a + b; // Doesn't have to be static, does have to be public.

// Somewhere in a method inside of that class
var concatDelegate = Dynamify.AutoDelegate("Concat", this); // Returns a Delegate type, can also use MethodInfo instead of the name.
// Can be then passed to the Dynamify.Make function
var dynamicConcat = Dynamify.Make(concatDelegate);
```

## What can I use this for?
You can use this for events that require arguments to be sent as an object array. The event end points don't need to be known at compile time and can be gathered at runtime for execution. This is the communication method that will be used for my message passing framework for Unity, *Pigeon*.

## How does it perform?
Much faster than `MethodInfo.Invoke()`, at least 7x faster. I will post performance numbers when the library is more completely. However, there is a high up front cost for the conversion.
