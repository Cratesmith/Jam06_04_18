using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
	public delegate float WeightFunction<T>(T value);
	
	/// Get a random item from the list.
	/// Random is sampled once. If random is null, UnityEngine.Random will be used
	public static T Random<T>(this IList<T> @this, System.Random random = null) 
	{
		if (@this.Count == 0)
		{
			return default(T);
		}
		int randomVal = random!=null ? random.Next(@this.Count):UnityEngine.Random.Range(0, @this.Count);
		return @this[randomVal];
	}

	/// Get a random item from the list, using a weighting function, Items with 0 or less weight are ignored. 
	/// Random is sampled once. If random is null, UnityEngine.Random will be used
	/// WeightFunction will be called twice on each item
	public static T Random<T>(this IList<T> @this, WeightFunction<T> weightFunction, System.Random random = null) 
	{
		if (@this.Count == 0)
		{
			return default(T);
		}

		float maxValue = 0f;
		for (int i = 0; i < @this.Count; i++)
		{
			var val = @this[i];
			maxValue += Mathf.Max(0,weightFunction(val));
		}

		if (maxValue <= 0)
		{
			return default(T);
		}

		T output = default(T);
		float currentValue = 0f;

		float randomVal = (random!=null ? (float)random.NextDouble():UnityEngine.Random.value) * (maxValue-Mathf.Epsilon)+Mathf.Epsilon;
		for (int i = 0; i < @this.Count; i++)
		{
			var current = @this[i];
			var weight = weightFunction(current);
			if (weight <= 0f)
			{
				continue;
			}

			output = current;
			currentValue += weight;
			if (randomVal <= currentValue)
			{
				break;
			}
		}

		return output;
	}
}