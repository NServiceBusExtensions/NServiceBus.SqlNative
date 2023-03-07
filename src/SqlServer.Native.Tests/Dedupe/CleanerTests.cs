[UsesVerify]
public class CleanerTests
{
    [Fact]
    public void If_triggers_critical_action_after_10_failures()
    {
        var criticalActionTriggered = false;
        var timer = new FakeTimer();
        var cleaner = new DedupeCleaner(_ => Task.FromResult(0),
            _ => criticalActionTriggered = true,  TimeSpan.Zero, timer);

        cleaner.Start();

        for (var i = 0; i < 9; i++)
        {
            timer.OnError(new("Simulated!"));
        }

        Assert.False(criticalActionTriggered);

        //Trigger the 10th time
        timer.OnError(new("Simulated!"));
        Assert.True(criticalActionTriggered);
        criticalActionTriggered = false;

        //Trigger again -- the counter should be reset
        timer.OnError(new("Simulated!"));
        Assert.False(criticalActionTriggered);
    }

    [Fact]
    public async Task It_resets_the_failure_counter_after_successful_attempt()
    {
        var criticalActionTriggered = false;
        var timer = new FakeTimer();
        var cleaner = new DedupeCleaner(_ => Task.FromResult(0),
            _ => criticalActionTriggered = true, TimeSpan.Zero, timer);

        cleaner.Start();

        for (var i = 0; i < 100; i++)
        {
            if (i % 9 == 0) //Succeed every 9th attempt
            {
                await timer.Tick(DateTime.UtcNow, CancellationToken.None);
            }
            else
            {
                timer.OnError(new("Simulated!"));
            }
        }

        Assert.False(criticalActionTriggered);
    }

    class FakeTimer :
        AsyncTimer
    {
        public Task Tick(DateTime utcTime, Cancellation cancellation) =>
            callback!(utcTime, cancellation);

        public void OnError(Exception error) =>
            errorCallback!(error);

        public override void Start(Func<DateTime, CancellationToken, Task> callback, TimeSpan interval, Action<Exception> errorCallback, Func<TimeSpan, CancellationToken, Task> delayStrategy)
        {
            this.callback = callback;
            this.errorCallback = errorCallback;
        }

        public override Task Stop() =>
            Task.FromResult(0);

        Func<DateTime, CancellationToken, Task>? callback;
        Action<Exception>? errorCallback;
    }
}