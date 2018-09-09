[Gtk.TreeNode]
public class CIAList : Gtk.TreeNode
{
    string file;
    string name;
    string description;
    public CIAList(string file, string name, string description)
    {
        this.file = file;
        this.name = name;
        this.description = description;
    }

    [Gtk.TreeNodeValue(Column = 0)]
    public string File { get { return file; }}

    [Gtk.TreeNodeValue(Column = 1)]
    public string Name { get { return name; } }

    [Gtk.TreeNodeValue(Column = 2)]
    public string Description { get { return description; } }
}