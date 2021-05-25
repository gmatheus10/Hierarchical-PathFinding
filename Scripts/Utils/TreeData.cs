using System.Collections;
using System.Collections.Generic;

public class TreeData<T> : IEnumerable<TreeData<T>>
{
    private readonly Dictionary<string, TreeData<T>> children = new Dictionary<string, TreeData<T>>();
    public readonly string ID;
    public TreeData<T> Parent { get; private set; }
    T Data { get; }
    public TreeData (string ID, T Data)
    {
        this.ID = ID;
        this.Data = Data;
    }
    public TreeData<T> GetChild (string ID)
    {
        return this.children[ID];
    }

    public void Add (TreeData<T> item)
    {
        if (item.Parent != null)
        {
            item.Parent.children.Remove( item.ID );
        }

        item.Parent = this;
        this.children.Add( item.ID, item );
    }
    public IEnumerator<TreeData<T>> GetEnumerator ( )
    {
        return this.children.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator ( )
    {
        return this.GetEnumerator();
    }

    public int Count { get { return this.children.Count; } }
}
