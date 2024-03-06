public class RowVersionTrackerTests :
    TestBase
{
    [Fact]
    public async Task Run()
    {
        await SqlConnection.DropTable(null, "RowVersionTracker");
        var tracker = new RowVersionTracker();
        await tracker.CreateTable(SqlConnection);
        var initial = await tracker.Get(SqlConnection);
        Assert.Equal(1, initial);
        await tracker.Save(SqlConnection,4);
        var after = await tracker.Get(SqlConnection);
        Assert.Equal(4, after);
    }
}