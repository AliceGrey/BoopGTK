using System;
using System.Configuration;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using rmortega77.CsHTTPServer;
using Gtk;

public partial class MainWindow : Gtk.Window
{
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
        hostipEntry.Text = GetLocalIPAddress();
      //  CsHTTPServer HTTPServer;
      //  System.Net.Sockets.Socket s; //Socket to tell FBI where the server is
      //  string[] FilesToBoop; //Files to be boop'd
      //  string ActiveDir; //Used to mount the server
        targetIP.Text = ConfigurationManager.AppSettings["saved3DSIP"];
        //rgetIP.Text;
        //  chkGuess.Checked = (bool)Properties.Settings.Default["bGuess"];
        //  txt3DS.Enabled = !chkGuess.Checked;
    }
  
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }


}
