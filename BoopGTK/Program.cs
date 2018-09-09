using System;
using Gtk;

namespace BoopGTK
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Application.Init();
            MainWindow win = new MainWindow();
            win.Show();
            Application.Run();
        }
        public static bool IsUnix()
        {
            int p = (int)Environment.OSVersion.Platform;
            if ((p == 4) || (p == 6) || (p == 128))
            {
                Console.WriteLine("Running on Unix");
                return true;
            }
            else
            {
                Console.WriteLine("NOT running on Unix");
                return false;
            }
        }
    }
}
