using Stride.Engine;

namespace R3.Stride.Sandbox
{
    class R3_Stride_SandboxApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
