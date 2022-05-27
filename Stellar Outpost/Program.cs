using System;
using System.Diagnostics;

namespace Stellar_Outpost
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            using var game = new Game1();
            game.Run();
        }
    }
}
