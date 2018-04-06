using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubEffectFlashMaterials : EffectComponent
{
	[SerializeField] private Material	m_flashMaterial;
	[SerializeField] private float		m_flashFrequency = .1f;
	[SerializeField] private float		m_duration = 1f;

	private bool m_isPlaying;
	private Dictionary<Renderer, Material[]>						m_previousMaterials = new Dictionary<Renderer, Material[]>();
	private static Dictionary<Renderer, SubEffectFlashMaterials>	s_flashEffectLookup = new Dictionary<Renderer, SubEffectFlashMaterials>();

	void Start()
	{
		if (m_flashMaterial != null && parent != null)
		{
			m_previousMaterials.Clear();
			StartCoroutine(EffectCoroutine());
		}
	}

	private IEnumerator EffectCoroutine()
	{
		m_isPlaying = true;
		m_previousMaterials = new Dictionary<Renderer, Material[]>();

		foreach (var renderer in parent.GetComponentsInChildren<Renderer>())
		{
		    if (!(renderer is MeshRenderer || renderer is SkinnedMeshRenderer)) continue;

			SubEffectFlashMaterials otherFlashEffect;
			if(s_flashEffectLookup.TryGetValue(renderer, out otherFlashEffect))
			{
				m_previousMaterials[renderer] = (Material[])otherFlashEffect.m_previousMaterials[renderer].Clone();
			}
			else
			{
				m_previousMaterials[renderer] = (Material[])renderer.sharedMaterials.Clone();
			}
			
			s_flashEffectLookup[renderer] = this;
		}

		while ((m_duration < 0 || (Time.time-owner.startTime <= m_duration)) && m_previousMaterials.Count > 0)
		{
			foreach (var kvp in m_previousMaterials)
			{
				if (kvp.Key != null)
				{
				    var sharedMaterials = kvp.Key.sharedMaterials;
				    for (int i = 0; i < sharedMaterials.Length; ++i)
				    {
					    sharedMaterials[i] = m_flashMaterial;
				    }
				    kvp.Key.sharedMaterials = sharedMaterials;
				}
			}
			yield return new WaitForSeconds(m_flashFrequency);
			if (owner.parent == null)
			{
				Stop();
				yield break;
			}

			ResetMaterials();
			yield return new WaitForSeconds(m_flashFrequency);
			if (owner.parent == null)
			{
				Stop();
				yield break;
			}
		}
        Stop();
	}

	private void ResetMaterials()
	{
		foreach (var kvp in m_previousMaterials)
		{
			if (kvp.Key != null)
			{
			    kvp.Key.sharedMaterials = kvp.Value;
			}
		}
	}

	public override void Stop()
	{
		ResetMaterials();
		foreach (var kvp in m_previousMaterials)
		{
			SubEffectFlashMaterials output;
			if (kvp.Key != null && s_flashEffectLookup.TryGetValue(kvp.Key, out output) && output==this)
			{
				s_flashEffectLookup.Remove(kvp.Key);
			}			
		}
		m_previousMaterials.Clear();
		m_isPlaying = false;
	}

	public override bool isPlaying
	{
		get { return m_isPlaying; }
	}
}