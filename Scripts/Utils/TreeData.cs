using System.Collections;
using System.Collections.Generic;

public class TreeData<T> : IEnumerable<TreeData<T>>
{
    public readonly Dictionary<int[,], TreeData<T>> Children = new Dictionary<int[,], TreeData<T>>();
    public readonly int[,] LevelBranch;
    public TreeData<T> Parent { get; private set; }
    public T Data { get; }
    public TreeData (int Level, int Branch, T Data)
    {
        int[,] LevelBranch = { { Level }, { Branch } };
        this.LevelBranch = LevelBranch;
        this.Data = Data;
    }
    public void Add (TreeData<T> item)
    {
        if (item.Parent != null)
        {
            item.Parent.Children.Remove( item.LevelBranch );
        }

        item.Parent = this;
        this.Children.Add( item.LevelBranch, item );
    }
    public TreeData<T> this[int Level, int Branch]
    {
        get
        {
            int[,] key = { { Level }, { Branch } };
            return Children[key];
        }
    }

    public IEnumerator<TreeData<T>> GetEnumerator ( )
    {
        return this.Children.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator ( )
    {
        return this.GetEnumerator();
    }

    public int Count { get { return this.Children.Count; } }

}
