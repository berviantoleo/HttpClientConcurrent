using System.Collections.Concurrent;
using System.Diagnostics;

var stopwatch = new Stopwatch();
using HttpClient client = new();

var semaphoreSlim = new SemaphoreSlim(initialCount: 10,
          maxCount: 10);
Console.WriteLine("{0} tasks can enter the semaphore.",
                          semaphoreSlim.CurrentCount);
var result = Enumerable.Range(0, 20_000);
var dictionaryResult = new ConcurrentBag<string>();
stopwatch.Start();
var tasks = result.Select(async x =>
{
    Console.WriteLine("Task {0} begins and waits for the semaphore.",
                                  x);
    int semaphoreCount;
    await semaphoreSlim.WaitAsync();
    try
    {
        Console.WriteLine("Task {0} enters the semaphore.", x);
        var response = await GetForecastAsync(x, client);
        dictionaryResult.Add(response);
    }
    finally
    {
        semaphoreCount = semaphoreSlim.Release();
    }
    Console.WriteLine("Task {0} releases the semaphore; previous count: {1}.",
                                  x, semaphoreCount);
});
Console.WriteLine("Waiting task");
await Task.WhenAll(tasks);
stopwatch.Stop();
Console.WriteLine(dictionaryResult.Count);
// Get the elapsed time as a TimeSpan value.
TimeSpan ts = stopwatch.Elapsed;

// Format and display the TimeSpan value.
string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
    ts.Hours, ts.Minutes, ts.Seconds,
    ts.Milliseconds / 10);
Console.WriteLine("RunTime " + elapsedTime);

static async Task<string> GetForecastAsync(int i, HttpClient client)
{
    Console.WriteLine($"Request from: {i}");
    var response = await client.GetAsync("http://localhost:5153/weatherforecast");
    return await response.Content.ReadAsStringAsync();
}