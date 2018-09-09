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
using BoopGTK;

public partial class MainWindow : Gtk.Window
{
    CsHTTPServer HTTPServer;
    System.Net.Sockets.Socket s; //Socket to tell FBI where the server is
    string FilesToBoop; //Files to be boop'd
    string ActiveDir; //Used to mount the server
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
        hostipEntry.Text = NetUtils.GetLocalIPAddress();
        targetIP.Text = ConfigurationManager.AppSettings["saved3DSIP"];
       
    }
    private void BtnFindIP_Clicked(object sender, EventArgs eventArgs)
    {
        string DSip = NetUtils.GetDSIP();
        Debug.Print(DSip);
        targetIP.Text = DSip;

    }
  


    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    protected void OnLocateDSbtnClicked(object sender, EventArgs e)
    {
        NetUtils.GetDSIP();
    }

    protected void btnFindIP_Clicked(object sender, EventArgs e)
    {
    }
}
