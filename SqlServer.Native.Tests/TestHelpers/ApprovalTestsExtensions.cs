using System.Threading.Tasks;

public static class ApprovalTestsExtensions
{
    public static void Wait(this Task task)
    {
        task.GetAwaiter().GetResult();
    }
}