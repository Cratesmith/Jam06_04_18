using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Effect : Actor
{
	private const float EFFECT_GRACE_TIME = 0.5f;
	private static Dictionary<Scene, Transform> s_effectSceneRoots = new Dictionary<Scene, Transform>();

	private List<EffectComponent>	m_effectComponents = new List<EffectComponent>();
    public ParticleSystem[]         particleSystems     { get; private set; }
    public TrailRenderer[]			trailRenderers		{ get; private set; }
	public AudioSource[]			audioSources		{ get; private set; }
	public Transform				parent				{ get; private set; }
	public float					startTime			{ get; private set; }
	public Vector3					localPosition		{ get; private set; }
	public Quaternion				localRotation		{ get; private set; }
	public bool						hasParent			{ get; private set; }
	public Rigidbody				rigidBody			{ get; private set; }
	public bool						isStopping			{ get; private set; }

	public static Effect Spawn<T>(T effectPrefab, Vector3 position, Quaternion rotation, Transform parent=null) where T:Effect
	{
		if (effectPrefab == null)
		{
			return null;
		}
	
		var effect = Instantiate(effectPrefab, position, rotation);
		effect.transform.parent = GetEffectRoot(effect.gameObject.scene);
		
		if (parent)
		{
			effect.parent			= parent;
			effect.localPosition	= parent.InverseTransformPoint(position);
			effect.localRotation	= rotation*Quaternion.Inverse(parent.rotation);
			effect.hasParent		= true;

			effect.rigidBody = effect.GetComponent<Rigidbody>();		
			if (!effect.rigidBody)
			{
				effect.rigidBody = effect.gameObject.AddComponent<Rigidbody>();
				effect.rigidBody.mass = 0f;
				effect.rigidBody.isKinematic = true;
				effect.rigidBody.useGravity = false;
			}
		}

		return effect;
	}

	private static Transform GetEffectRoot(Scene scene)
	{
		Transform output;
		if (!s_effectSceneRoots.TryGetValue(scene, out output))
		{
			output = s_effectSceneRoots[scene] = new GameObject("Effects").transform;
			SceneManager.MoveGameObjectToScene(output.gameObject, scene);
		}
		return output;
	}

	void Awake()
	{
		particleSystems = GetComponentsInChildren<ParticleSystem>();
        trailRenderers = GetComponentsInChildren<TrailRenderer>();
		audioSources = GetComponentsInChildren<AudioSource>();
		startTime = Time.time;
	}

	void LateUpdate()
	{
		if (parent && rigidBody)
		{
			var prevPosition = transform.position;
			var newPosition = transform.position = parent.TransformPoint(localPosition);

			// we just want to match position & rotation, 
			// but we're using rigidbody velocity to get around a 2017.2 
			// bug where particle systems can ONLY read velocity from rigidbodies
			rigidBody.isKinematic = false;
			rigidBody.useGravity = false;
			rigidBody.velocity = (newPosition - prevPosition)/Time.deltaTime; 
			transform.rotation = parent.rotation * localRotation;
			
		}
		else if (hasParent)
		{
			if (rigidBody)
			{
				rigidBody.isKinematic = true;
				rigidBody.velocity = Vector3.zero;				
			}

			Stop();
		}

        if(isStopping)
        {
            foreach(var trailRenderer in trailRenderers)
            {
                trailRenderer.time = Mathf.Lerp(trailRenderer.time, 0, Time.deltaTime*4f);
            }
        }

		if (Time.time - startTime < EFFECT_GRACE_TIME)
		{
			return;
		}

		foreach (var system in particleSystems)
		{
			if ((system.emission.enabled && system.main.loop) || system.particleCount > 0)
			{
				return;
			}
		}

		foreach (var audioSource in audioSources)
		{
			if (audioSource.isPlaying)
			{
				return;
			}
		}

		foreach (var effectComponent in m_effectComponents)
		{
			if (effectComponent.isPlaying)
			{
				return;
			}
		}

        foreach (var trailRenderer in trailRenderers)
        {
            if (trailRenderer.positionCount > 0 && trailRenderer.time > 0f)
            {
                return;
            }
        }

		Destroy(gameObject);
	}

	public void Stop()
	{
		if (isStopping) return;
		isStopping = true;
	
		foreach (var system in particleSystems)
		{
			var emmision = system.emission;
			emmision.enabled = false;
		}

		foreach (var audioSource in audioSources)
		{
			audioSource.loop = false;
		}

		foreach (var effectComponent in m_effectComponents)
		{
			effectComponent.Stop();
		}
	}

	public void AddEffectComponent(EffectComponent effectComponent)
	{
		m_effectComponents.Add(effectComponent);
	}

	public void RemoveEffectComponent(EffectComponent effectComponent)
	{
		m_effectComponents.Remove(effectComponent);
	}
}