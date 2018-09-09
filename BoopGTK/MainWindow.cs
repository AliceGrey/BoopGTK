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
   // CsHTTPServer HTTPServer;
    // System.Net.Sockets.Socket s; //Socket to tell FBI where the server is
    string[] FilesToBoop; //Files to be boop'd
    string ActiveDir; //Used to mount the server
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
        hostipEntry.Text = NetUtils.GetLocalIPAddress();
        targetIP.Text = ConfigurationManager.AppSettings["savedDSIP"];

        // store.AddNode(new CIAList("Peter Gabriel"));
        // store.AddNode(new CIAList("U2"));

        nodeview1.AppendColumn("File", new Gtk.CellRendererText(), "text", 0);
        nodeview1.AppendColumn("Name", new Gtk.CellRendererText(), "text", 1);
        nodeview1.AppendColumn("Description", new Gtk.CellRendererText(), "text", 2);
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
    /// <summary>
    /// Processes The Files
    /// </summary>
    private void ProcessFilenames()
    {
        ActiveDir = (System.IO.Path.GetDirectoryName(FilesToBoop[0]));
        string file = "";
        string name = "";
        string description = "";

        foreach (string item in FilesToBoop)
        {
            if (ActiveDir == System.IO.Path.GetDirectoryName(item))
            {
                if (System.IO.Path.GetExtension(item) == ".cia")
                {
                    byte[] desc = new Byte[256];

                    byte[] tit = new Byte[128];

                    using (BinaryReader b = new BinaryReader(File.Open(item, FileMode.Open)))
                    {
                        b.BaseStream.Seek(-14016 + 520, SeekOrigin.End);
                        tit = b.ReadBytes(128);

                        b.BaseStream.Seek(-14016 + 520 + 128, SeekOrigin.End);
                        desc = b.ReadBytes(256);
                    }

                    file = System.IO.Path.GetFileName(item);
                    name = Encoding.Unicode.GetString(tit).Trim();
                    description = Encoding.Unicode.GetString(desc).Trim();

                    NodeStore store = new NodeStore(typeof(CIAList));
                    store.AddNode(new CIAList(file, name, description));
                    nodeview1.NodeStore = store;
                }
                else
                {
                    /*TODO:Throw an error?*/
                }

            }
            else
            {


                MessageDialog md = new MessageDialog(this,
                                                     DialogFlags.DestroyWithParent,
                                                     MessageType.Error, ButtonsType.Ok,
                                                     "You picked 2 files that are NOT in the same directory" + Environment.NewLine + "Cross-Directory booping would need the entire computer hosted to the network and that doesn't feel safe in my book." + Environment.NewLine + "Maybe in the future I'll find a way to do this.");
                md.Run();
                md.Destroy();


            }
        }
    }


    protected void OnPickFilesClicked(object sender, EventArgs e)
    {

        // Create an instance of the open file dialog box.
        // lblFileMarker.Visible = false;
        FileChooserDialog OFD = new FileChooserDialog(
            "Please select the cia's to boop",
            this,
            FileChooserAction.Open,
            "Cancel", ResponseType.Cancel,
            "Open", ResponseType.Accept);
        OFD.SelectMultiple = true;
        FileFilter filter = new FileFilter();
        filter.Name = "FBI compatible files";
        filter.AddPattern("*.cia");
        OFD.AddFilter(filter);

        if (OFD.Run() == (int)ResponseType.Accept)
        {
            if (OFD.Filenames.Length > 0)
            {
                FilesToBoop = OFD.Filenames.ToArray();
                ProcessFilenames();
            }
        }
        // end if
        OFD.Destroy();
    }
}
