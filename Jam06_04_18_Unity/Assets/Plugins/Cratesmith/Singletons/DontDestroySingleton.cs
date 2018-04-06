using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Base for auto-constructed singletons that aren't tied to a scene
/// These singletons usually persist for the application's entire lifecycle once created
/// </summary>
/// <typeparam name="TSelfType"></typeparam>
[ScriptExecutionOrder(-200)] // base exection order -100 (before normal objects)
public abstract class DontDestroySingleton<TSelfType> : DontDestroySingletonBase where TSelfType:DontDestroySingleton<TSelfType>
{	
	private static TSelfType s_instance;
	private bool m_initialized;

	protected virtual void Awake()
	{
		if (s_instance != null && s_instance != this)
		{
			Debug.LogErrorFormat("A second instance of {0} is being created in scene {1}", typeof(TSelfType).Name, gameObject.scene.name);
			DestroyImmediate(this);
		}

		// we set the instance in awake so that other Awake calls in the same frame don't create multiple instances of the singleton
		s_instance = (TSelfType)this;
		m_initialized = true;
	}  

	public static TSelfType Get(bool constructIfMissing=true)
	{
		if (!s_instance && constructIfMissing)
		{
			var go = new GameObject(typeof(TSelfType).Name);
			go.transform.parent = GetSingletonRoot();
			s_instance = go.AddComponent<TSelfType>(); 
		}
		Assert.IsTrue(s_instance==null || s_instance.m_initialized, string.Format("Singleton {0} wasn't initialized! Did you override Awake() and forget to call base.Awake()?", typeof(TSelfType).FullName));
		return s_instance;
	}
}

// A base class that provides the root object for singletons in the dontdestroy scene
public abstract class DontDestroySingletonBase : Actor
{
	private static Transform s_dontDestroyRoot;

	protected static Transform GetSingletonRoot(bool constructIfMissing=true)
	{
		if (!s_dontDestroyRoot && constructIfMissing)
		{
			s_dontDestroyRoot = new GameObject("Singletons").transform;
			DontDestroyOnLoad(s_dontDestroyRoot);
		}
		return s_dontDestroyRoot;
	}
}