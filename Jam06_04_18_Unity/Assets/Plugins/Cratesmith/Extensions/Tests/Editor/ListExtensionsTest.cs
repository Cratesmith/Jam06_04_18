using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ListExtensionsTest 
{
	[Test]
	public void TestRandom()
	{
		{
			// empty list random = default(T)
			var emptyList = new List<int>();
			Assert.AreEqual(emptyList.Random(), 0); 
		}

		{
			// remove each item in random order. No removals should fail
			var removeList = System.Linq.Enumerable.Range(0, 10).ToList();
			int count = removeList.Count;
			for (int i = 0; i < count; i++)
			{
				var id = removeList.Random();
				removeList.Remove(id);		
			}		
			Assert.AreEqual(0, removeList.Count); 			
		}

		{			
			var longList = System.Linq.Enumerable.Range(0, 10).Reverse().ToList();
			for (int i = 0; i < longList.Count; i+=2)
			{
				longList.Add(0);
			}

			var hits = new int[longList.Max()+1];
			for (int i = 0; i < 10000; i++)
			{
				var id = longList.Random(x=>x);
				++hits[id];
			}

			Assert.AreEqual(0, hits[0]);
			Assert.Greater(hits.Take(longList.Count/2).Sum(), hits.Skip(longList.Count/2).Sum());
		}
	}
}
