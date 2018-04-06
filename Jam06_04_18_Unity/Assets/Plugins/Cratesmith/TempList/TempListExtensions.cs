using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
// Extension methods for using TempLists to handle GetComponentsIn... calls
//
// example usage:
//
// using(var rigidbodies = obj.GetComponentsInChildrenTempList<Rigidbody>())
// foreach (var rigidbody in rigidbodies)
// {
//     Debug.Log(rigidbody.name);
// }
public static class TempListExtensions
{
    public static TempList<T> GetComponentsTempList<T>(this Component @this)
    {
        var tempList = TempList<T>.Get();
        if (@this != null)
        {
            @this.GetComponents<T>(tempList.list);
        }
        return tempList;
    }

	public static TempList<T> GetComponentsInChildrenTempList<T>(this Component @this, bool includeInactive=false)
	{
		var tempList = TempList<T>.Get();
		if(@this!=null)
		{
			@this.GetComponentsInChildren<T>(includeInactive, tempList.list);
		}
		return tempList;
	}
 
	public static TempList<T> GetComponentsInParentTempList<T>(this Component @this, bool includeInactive=false)
	{
		var tempList = TempList<T>.Get();
		if(@this!=null)
		{
			@this.GetComponentsInParent<T>(includeInactive, tempList.list);
		}
		return tempList;
	}

    public static TempList<T> GetComponentsTempList<T>(this GameObject @this)
    {
        var tempList = TempList<T>.Get();
        if (@this != null)
        {
            @this.GetComponents<T>(tempList.list);
        }
        return tempList;
    }
 
	public static TempList<T> GetComponentsInChildrenTempList<T>(this GameObject @this, bool includeInactive=false)
	{
		var tempList = TempList<T>.Get();
		if(@this!=null)
		{
			@this.GetComponentsInChildren<T>(includeInactive, tempList.list);
		}
		return tempList;
	}
 
	public static TempList<T> GetComponentsInParentTempList<T>(this GameObject @this, bool includeInactive=false)
	{
		var tempList = TempList<T>.Get();
		if(@this!=null)
		{
			@this.GetComponentsInParent<T>(includeInactive, tempList.list);
		}
		return tempList;
	}
}