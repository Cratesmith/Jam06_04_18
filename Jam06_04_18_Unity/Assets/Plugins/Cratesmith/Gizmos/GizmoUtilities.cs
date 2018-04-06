using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GizmoUtilities 
{
	public static void DrawPrefab(GameObject prefab, Transform transform)
	{
		if (prefab == null)
		{
			return;
		}

		var prevMatrix = Gizmos.matrix;
		Gizmos.matrix = transform.localToWorldMatrix;
		foreach (var meshFilter in prefab.GetComponentsInChildren<MeshFilter>())
		{			
			Gizmos.DrawMesh(meshFilter.sharedMesh, Vector3.zero, Quaternion.identity);
		}
		Gizmos.matrix = prevMatrix;
	}
}
