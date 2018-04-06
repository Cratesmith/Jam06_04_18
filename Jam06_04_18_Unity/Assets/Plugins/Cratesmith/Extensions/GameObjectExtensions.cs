using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cratesmith
{
	public static class GameObjectExtensions
	{
		public static T GetOrAddComponent<T>(this GameObject @this) where T:Component
		{
			T output = @this.GetComponent<T>();
			if (output == null)
			{
				output = @this.AddComponent<T>();
			}
			return output;
		}

		public static TRequired GetOrAddComponent<TRequired, TDefault>(this GameObject @this) where TRequired:Component where TDefault:TRequired
		{
			TRequired output = @this.GetComponent<TRequired>();
			if (output == null)
			{
				output = @this.AddComponent<TDefault>();
			}
			return output;
		}
	}
}
