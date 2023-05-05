# CloudVars

A thread-safe, in-memory key-value store with optional expiration and callback support.

---

## Features

- Thread-safe operations
- Singleton access
- Key-value storage with optional expiration
- Callbacks for value changes
- Save and load data to/from a file

---

## Table of Contents

- [Dependencies](#dependencies)
- [Usage](#usage)
  - [Basic usage](#basic-usage)
  - [Expiration](#expiration)
  - [Callbacks](#callbacks)
  - [Save and load data](#save-and-load-data)
- [More advanced examples](#advanced-examples)
- [API](#api)

---

## Dependencies

- System.Threading
- System.Collections.Concurrent
- System.Runtime.Serialization
- System.IO
- Newtonsoft.Json (for JSON serialization)

---

## Usage

### Basic usage

```csharp
var cloudVars = CloudVars.Instance;

// Add a new key-value pair
cloudVars.Add("exampleKey", "exampleValue");

// Get a value by key
string value = cloudVars.Get<string>("exampleKey");

// Set a value by key
await cloudVars.SetAsync("exampleKey", "newValue");

// Check if a key exists
bool exists = cloudVars.Contains("exampleKey");

// Remove a key
cloudVars.Remove("exampleKey");
```

### Expiration

```csharp
var cloudVars = CloudVars.Instance;

// Add a value with expiration
cloudVars.Add("expiringKey", "expiringValue", TimeSpan.FromSeconds(30));

// Set a value with expiration
await cloudVars.SetAsync("exampleKey", "newValueWithExpiration", TimeSpan.FromSeconds(30));
```

### Callbacks

```csharp
var cloudVars = CloudVars.Instance;

// Register a callback
cloudVars.RegisterCallback("exampleKey", (value) => {
    Console.WriteLine($"New value: {value}");
    return Task.CompletedTask;
});


// Register an async callback
cloudVars.RegisterCallback("exampleKey", async (value) => {
    await Task.Delay(100);
    Console.WriteLine($"New value after delay: {value}");
});

// Remove a callback
cloudVars.RemoveCallback("exampleKey", callback);

// Remove all callbacks for a key
cloudVars.RemoveAllCallbacks("exampleKey");
```

### Save and load data

```csharp
var cloudVars = CloudVars.Instance;

// Save data to a file
await cloudVars.SaveToFileAsync("data.json");

// Load data from a file
await cloudVars.LoadFromFileAsync("data.json");
```

---

## Advanced examples

### 1. Real-time notifications

Demonstrates how to use CloudVars to send real-time notifications to users in a multi-user application.

```csharp
var cloudVars = CloudVars.Instance;

// Send a notification to a user
string userId = "user1";
string notification = "You have a new message!";
cloudVars.Add($"notification_{userId}", notification, TimeSpan.FromSeconds(30));

// Register a callback to receive notifications
cloudVars.RegisterCallback($"notification_{userId}", (value) =>
{
    Console.WriteLine($"Received notification: {value}");
    return Task.CompletedTask;
});
```

### 2. Caching API responses

Demonstrates how to cache API responses using CloudVars with expiration to automatically refresh the cache.

```csharp
var cloudVars = CloudVars.Instance;

async Task<string> FetchFromApiAsync()
{
    // Simulate an API call
    await Task.Delay(1000);
    return "API response";
}

async Task<string> GetCachedApiResponseAsync()
{
    const string cacheKey = "apiResponse";

    if (!cloudVars.Contains(cacheKey))
    {
        string response = await FetchFromApiAsync();
        cloudVars.Add(cacheKey, response, TimeSpan.FromMinutes(1));
    }

    return cloudVars.Get<string>(cacheKey);
}

string cachedResponse = await GetCachedApiResponseAsync();
Console.WriteLine($"Cached response: {cachedResponse}");
```

### 3. Asynchronous task queue

Demonstrates how to use CloudVars to create an asynchronous task queue with real-time progress updates.

```csharp
var cloudVars = CloudVars.Instance;

async Task ProcessTaskAsync(int taskId)
{
    await Task.Delay(1000);
    Console.WriteLine($"Task {taskId} completed");
}

async Task AddTaskToQueueAsync(int taskId)
{
    cloudVars.Add($"task_{taskId}", taskId);
    await ProcessTaskAsync(taskId);
    cloudVars.Remove($"task_{taskId}");
}

cloudVars.RegisterCallback("task_*", (taskId) =>
{
    Console.WriteLine($"Task {taskId} added to queue");
    return Task.CompletedTask;
});

// Add tasks to the queue
await AddTaskToQueueAsync(1);
await AddTaskToQueueAsync(2);
await AddTaskToQueueAsync(3);
```

---

## API

- `Add(string name, object value, TimeSpan? expiration = null)`: Adds a new key-value pair.
- `Get<T>(string name)`: Gets the value associated with the specified key.
- `Get<T>(string name, T defaultValue)`: Gets the value associated with the specified key, or the default value if the key does not exist.
- `SetAsync(string name, object value, TimeSpan? expiration = null)`: Sets the value of the specified key.
- `Remove(string name)`: Removes the specified key and its associated value.
- `Contains(string name)`: Determines whether the store contains the specified key.
- `OnChange(string name, Action<object> callback)`: Registers a callback to be called when the specified key's value changes.
- `OnChange(string name, Func<object, Task> callback)`: Registers an async callback to be called when the specified key's value changes.
- `RemoveCallback(string name, Func<object, Task> callback)`: Removes the specified callback for the specified key.
- `RemoveAllCallbacks(string name)`: Removes all callbacks for the specified key.
- `SaveToFileAsync(string filePath)`: Saves the current data to the specified file.
- `LoadFromFileAsync(string filePath)`: Loads data from the specified file and updates the store.



---

## License

CloudVars is released under the MIT License. See the [LICENSE](LICENSE) file for more information.