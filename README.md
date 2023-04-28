# CloudVars Documentation

## Overview

CloudVars is a C# class designed to provide a simple and thread-safe way to manage global variables in your code. With CloudVars, you can add, set, get, remove and check if a variable exists, all from a single location, without having to define global variables in different parts of your code. CloudVars also provides support for registering callbacks that are invoked when the value of a variable changes, as well as support for setting expiration times for variables and performing operations atomically using transactions. 

## Installation

To use CloudVars, simply copy `CloudVars.cs` and `CV.cs` into your project's folder, and add `using CloudVar;` to your code file. Alternatively, you can add the binary file to your project's references.


## Usage

To use the `CV` class, simply call the static methods provided by the class.

### Add

Adds a new value with the specified name to the cloud variables.

```csharp
public static void Add(string name, object value)
```

### Get

Gets the value of the specified cloud variable.

```csharp
public static T Get<T>(string name)
```

### SetAsync

Sets the value of the specified cloud variable asynchronously.

```csharp
public static async Task SetAsync(string name, object value)
```

### AddRange

Adds multiple new key-value pairs to the cloud.

```csharp
public static void AddRange(IDictionary<string, object> values)
```

### SetRangeAsync

Updates the values of multiple existing keys in the cloud asynchronously.

```csharp
public static async Task SetRangeAsync(IDictionary<string, object> values)
```

These methods allow adding or updating multiple key-value pairs at once. The `AddRange` method takes an `IDictionary<string, object>` as a parameter and adds each key-value pair to the cloud. The SetRangeAsync method also takes an `IDictionary<string, object>` as a parameter and updates each key-value pair in the cloud.


### Example
```csharp
// Create a mixed dictionary of key-value pairs to add to the cloud
var valuesToAdd = new Dictionary<string, object>
{
    { "clientValue", new HttpClient() },
    { "messageValue", new HttpRequestMessage(HttpMethod.Get, "https:...") }
};

// Add the key-value pairs to the cloud
CV.AddRange(valuesToAdd);

// Create a mixed dictionary of key-value pairs to update in the cloud
var valuesToUpdate = new Dictionary<string, object>
{
    { "clientValue", new HttpClient() },
    { "messageValue", new HttpRequestMessage(HttpMethod.Post, "https://...") }
};

// Update the key-value pairs in the cloud
await CV.SetRangeAsync(valuesToUpdate);
```



### Contains

Determines whether the specified cloud variable exists.

```csharp
public static bool Contains(string name)
```

### OnChange (Action)

Registers a callback to be executed when the specified cloud variable is changed.

```csharp
public static void OnChange(string name, Action<object> callback)
```

### OnChange (Func)

```csharp
public static void OnChange(string name, Func<object, Task> callback)
```

The two OnChange methods in the CV class allow registering callbacks to be called when the value of a cloud variable changes.

The first OnChange method takes an `Action<object>` callback, which is a synchronous callback that is executed when the value of the cloud variable changes. This method is useful if the callback doesn't need to perform any asynchronous operations.

The second OnChange method takes a `Func<object, Task>` callback, which is an asynchronous callback that is executed when the value of the cloud variable changes. This method is useful if the callback needs to perform asynchronous operations, such as calling an API or accessing a database.

*In general, use the first OnChange method with synchronous callbacks that don't need to perform any asynchronous operations. Use the second OnChange method with asynchronous callbacks that need to perform asynchronous operations.*

### Remove

Removes the specified cloud variable.

```csharp
public static void Remove(string name)
```

### GetAllNames

Returns a list of all names of keys in cloud.

```csharp
public static List<string> GetAllNames()
```

### Clear

Clears all keys and values from cloud.

```csharp
public static void Clear()
```

### RemoveCallback

Removes specified callback for specified key.

```csharp
public static void RemoveCallback(string name, Func<object, Task> callback)
```

### RemoveAllCallbacks

Removes all callbacks for specified key.

```csharp
public static void RemoveAllCallbacks(string name)
```



### Responding to changes in a variable

Registering a callback to be called whenever a variable changes value. Use the `OnChange` method, which takes two arguments: the name of the variable as a string, and an `Action<object>` callback that will be called whenever the variable changes.

```csharp
CV.OnChange("variableName", (value) => Console.WriteLine("Variable value changed to: " + value));
```

## Example

```csharp
using CloudVar;

var c1 = new class1();
var c2 = new class2();

_ = CV.SetAsync("class2", 15);

Console.WriteLine(CV.Get<int>("class1").ToString() + "  " + CV.Get<int>("class2").ToString());
Console.WriteLine(c1.number.ToString() + "  " + c2.number.ToString());



class class1
{
    public int number;

    public class1()
    {
        number = 5;
        CV.Add("class1", number);
        CV.OnChange("class1", (value) => number = (int)value);
    }
}

class class2
{
    public int number;

    public class2()
    {
        number = 10;
        CV.Add("class2", number);
        CV.OnChange("class2", (value) => number = (int)value);
    }
}
```

Output:
``` 
5 15
5 15
```


### SetAsyncConcurrent

Sets the value of the specified cloud variable asynchronously and executes all registered callbacks concurrently.

```csharp
public static async Task SetAsyncConcurrent(string name, object value)
```
### SetRangeAsyncConcurrent

Updates the values of multiple existing keys in the cloud asynchronously and executes all registered callbacks for each key concurrently.

```csharp
public static async Task SetRangeAsyncConcurrent(IDictionary<string, object> values)
```
These methods provide an alternative to the `SetAsync` and `SetRangeAsync` methods that execute callbacks *concurrently* instead of *serially*. You can use these methods when you want to update a value or multiple values and execute their callbacks concurrently.


## Expiration time

The `CloudVars` class provides support for setting an expiration time for keys. This allows you to specify a `TimeSpan` after which the key will automatically expire and be removed from the store.

To set an expiration time for a key, you can use the `add`, `setAsync`, or `setAsyncConcurrent` methods and provide a value for the optional `expiration` parameter. For example:

```csharp
// Add a new key-value pair to the store with an expiration time of 5 minutes
CV.Add("myKey", "myValue", TimeSpan.FromMinutes(5));

// Update the value of an existing key and set its expiration time to 10 minutes
await CV.SetAsync("myKey", "newValue", TimeSpan.FromMinutes(10));
```

When you retrieve the value of a key using one of the `get` methods, the `CloudVars` class will automatically check if the key has expired and remove it from the store if necessary.

```csharp
// Get the value of a key
var value = CV.Get<string>("myKey");

// If the key has expired, an exception will be thrown
```

The ability to set an expiration time for keys can be useful in scenarios where you want to store temporary data that should be automatically removed after a certain period of time.



Example that demonstrates the transaction features of the `CloudVars` class using the `CV` wrapper class:


**Performing operations atomically using transactions:**

```csharp
using (var scope = new TransactionScope())
{
    // Add a new key-value pair to the store
    CV.Add("myKey1", "myValue1");

    // Update the value of an existing key
    await CV.SetAsync("myKey2", "newValue2");

    // Complete the transaction
    scope.Complete();
}
```

In the last example, we used a transaction to perform two operations on the `CloudVars` store ("cloud") atomically: adding a new key-value pair and updating the value of an existing key. This means that either both operations are performed successfully or neither of them is performed at all.

By using a transaction, you can ensure that both operations are performed atomically, so that if one of the operations fails, the other operation is automatically rolled back and the data remains consistent.



## License

CloudVars is licensed under the MIT License. See the [License.md](License.md) file for details.
