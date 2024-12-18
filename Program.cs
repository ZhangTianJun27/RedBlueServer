using RedBlue_Server.Server;

namespace RedBlue_Server;

public class Program
{
    private static void Main(string[] args)
    {
        ServerRoot.Instance.Init();

        while (true)
        {
            ServerRoot.Instance.Update();
            Thread.Sleep(10);
        }
    }
}