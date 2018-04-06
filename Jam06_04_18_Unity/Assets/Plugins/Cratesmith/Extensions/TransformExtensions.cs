using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions 
{
	public static void Reset(this Transform @this)
	{
		@this.localPosition = Vector3.zero;
		@this.localRotation = Quaternion.identity;
		@this.localScale = Vector3.one;
	}
}
