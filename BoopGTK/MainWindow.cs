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
using Gtk;
using BoopGTK;
using rmortega77.CsHTTPServer;

public partial class MainWindow : Gtk.Window
{
    CsHTTPServer HTTPServer;
    System.Net.Sockets.Socket s; //Socket to tell FBI where the server is
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
        //Individual trycatches to make sure everything is off before leaving.
        try
        {
            HTTPServer.Stop();
        }
        catch { }

        try
        {
            s.Close();
        }
        catch { }
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

    protected void OnBoopBtnClicked(object sender, EventArgs e)
    {
       
           

        //TODO: Check network connection

            try
            {
              

                //Fastest check first.
            if (!FilesToBoop.Any())
                {
                //TODO: Throw error
                    return;
                }
               
            if (NetUtils.Validate(targetIP.Text) == false)
                    {
                //TODO: Throw error
                        return;
                    }

            string DSip = targetIP.Text;
            int ServerPort = Int32.Parse(Port.Text);

                // THE FIREWALL IS NO LONGER POKED!
                // THE SNEK IS FREE FROM THE HTTPLISTENER TIRANY!

               // setStatusLabel("Opening the new and improved snek server...");
             //   enableControls(false);
            Console.WriteLine("Active Dir");
            Console.WriteLine(ActiveDir);
            Console.WriteLine("Port");
            Console.WriteLine(ServerPort.ToString());
            string SafeDir = ActiveDir + "/";


            HTTPServer = new MyServer(ServerPort, ActiveDir);
                HTTPServer.Start();


                System.Threading.Thread.Sleep(100);

              //  setStatusLabel("Opening socket to send the file list...");

                s = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IAsyncResult result = s.BeginConnect(DSip, 5000, null, null);
                result.AsyncWaitHandle.WaitOne(5000, true);

                if (!s.Connected)
                {
                    s.Close();
                    HTTPServer.Stop();
                  //  MessageBox.Show("Failed to connect to 3DS" + Environment.NewLine + "Please check:" + Environment.NewLine + "Did you write the right IP adress?" + Environment.NewLine + "Is FBI open and listening?", "Connection failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 //   lblIPMarker.Visible = true;
                //    setStatusLabel("Ready");
                //    enableControls(true);
                    return;
                }

              //  setStatusLabel("Sending the file list...");

                String message = "";
            String formattedPort = ":" + Port.Text + "/";

                foreach (var CIA in FilesToBoop)
                {
                message += NetUtils.GetLocalIPAddress() + formattedPort + System.Web.HttpUtility.UrlEncode(System.IO.Path.GetFileName(CIA)) + "\n";
                }

                //boop the info to the 3ds...
                byte[] Largo = BitConverter.GetBytes((uint)Encoding.ASCII.GetBytes(message).Length);
                byte[] Adress = Encoding.ASCII.GetBytes(message);

                Array.Reverse(Largo); //Endian fix

                s.Send(AppendTwoByteArrays(Largo, Adress));

               // setStatusLabel("Booping files... Please wait");
                s.BeginReceive(new byte[1], 0, 1, 0, new AsyncCallback(GotData), null); //Call me back when the 3ds says something.

                //#if DEBUG
            }
            catch (Exception ex)
            {
                //Hopefully, some day we can have all the different exceptions handled... One can dream, right? *-*
             //   MessageBox.Show("Something went really wrong: " + Environment.NewLine + Environment.NewLine + "\"" + ex.Message + "\"" + Environment.NewLine + Environment.NewLine + "If this keeps happening, please take a screenshot of this message and post it on our github." + Environment.NewLine + Environment.NewLine + "The program will close now", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Quit();
            }
            //#endif

    }
    private void GotData(IAsyncResult ar)
    {

        // now we unlock the controls...
        //Spooky "thread safe" way to access UI from ASYNC.
        Gtk.Application.Invoke(delegate
        {
            MessageDialog md = new MessageDialog(this,
                                                    DialogFlags.DestroyWithParent,
                                                    MessageType.Error, ButtonsType.Ok,
                                                    "Booping complete!");
            md.Run();
            md.Destroy();
          //  enableControls(true);
        });

        s.Close();
        HTTPServer.Stop();
    }

    static byte[] AppendTwoByteArrays(byte[] arrayA, byte[] arrayB) //Aux function to append the 2 byte arrays.
    {
        byte[] outputBytes = new byte[arrayA.Length + arrayB.Length];
        Buffer.BlockCopy(arrayA, 0, outputBytes, 0, arrayA.Length);
        Buffer.BlockCopy(arrayB, 0, outputBytes, arrayA.Length, arrayB.Length);
        return outputBytes;
    }
}
