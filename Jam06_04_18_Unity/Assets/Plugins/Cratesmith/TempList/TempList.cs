// Cratesmith 2017
using System;
using System.Collections;
using System.Collections.Generic;

// Temporary list pool for use with unity api
//
// eg: 
// using (var list = TempList<Renderer>.Get()) 
// {
//		transform.GetComponentsInChildren<Renderer>(list);
//		foreach(var r in list) Debug.Log(r.name);
// }
public class TempList<T> : IDisposable, IList<T>
{
    private static readonly Queue<TempList<T>> s_lists = new Queue<TempList<T>>();
	public readonly List<T> list = new List<T>();

	/// constructor is private. Use satic Get method instead
	private TempList() {}

	// acquire a temporary list
	public static TempList<T> Get()
	{
        lock(s_lists)
        {            
    	    return s_lists.Count > 0
    		    ? s_lists.Dequeue()
    		    : new TempList<T>();
        }
	}

	// return a list back to the pool
	public void Dispose()
	{
		list.Clear();
        lock(s_lists)
        {
		    s_lists.Enqueue(this);
        }
	}

	public static implicit operator List<T>(TempList<T> from)
	{
		return from != null ? from.list : null;
	}

	#region IList implementation
	public IEnumerator<T> GetEnumerator()
	{
		return list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable) list).GetEnumerator();
	}

	public void Add(T item)
	{
		list.Add(item);
	}

	public void Clear()
	{
		list.Clear();
	}

	public bool Contains(T item)
	{
		return list.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		list.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		return list.Remove(item);
	}

	public int Count
	{
		get { return list.Count; }
	}

	public bool IsReadOnly
	{
		get { return false; }
	}

	public int IndexOf(T item)
	{
		return list.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		list.Insert(index, item);
	}

	public void RemoveAt(int index)
	{
		list.RemoveAt(index);
	}

	public T this[int index]
	{
		get { return list[index]; }
		set { list[index] = value; }
	}
	#endregion

}