using Client.Enums;

namespace Client
{
    public interface IChatClient
    {
        bool Discord{ get; set; }
        string DisplayName { get; set; }
        
        ClientState State { get; set; }
        Task RunAsync();
        Task SendMessageAsync(byte[] message);
        Task ShutdownAsync();
        
        // Console.ReadLine is blocking function and without this helper function
        // the Console.ReadLine would leve the program hanging when trying to exit  
        private static async Task<string?> ReadLineAsync(CancellationToken token)
        {
            // Wrap Console.ReadLine in a task and race against a cancellation delay.
            var inputTask = Task.Run(Console.ReadLine, token);
            var cancelTask = Task.Delay(Timeout.Infinite, token);
            var completed = await Task.WhenAny(inputTask, cancelTask);
            if (completed == cancelTask)
                throw new OperationCanceledException(token);
            return await inputTask;
        }
    }
}