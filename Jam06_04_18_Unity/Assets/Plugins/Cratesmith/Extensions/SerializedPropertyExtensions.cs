using System.Reflection;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;


public static class SerializedPropertyExtensions 
{
	public static System.Type GetSerializedPropertyType(this SerializedProperty @this)
	{
		// follow reflection up to match path and return type of last node

		// fix path for arrays
		var path = @this.propertyPath.Replace(".Array.data[", "[");
	
		var currentType = @this.serializedObject.targetObject.GetType();

		string[] slices = path.Split('.', '[');
		foreach (var slice in slices)
		{
			// array element: get array type if this is an array element
			if (slice.EndsWith("]"))
			{
				currentType = currentType.GetElementType();
			}
			else // field: find field by same name as slice and match to type
			{
				var type = currentType;
				while (type != null)
				{
					var fieldInfo = type.GetField(slice, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (fieldInfo == null)
					{
						type = type.BaseType;
						continue;
					}

					currentType = fieldInfo.FieldType;
					break;
				}
				Assert.IsNotNull(type);
			}
		}

		return currentType;
	}
}
#endif