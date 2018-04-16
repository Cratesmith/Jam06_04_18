using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorPickupCoin : Actor
{
    [SerializeField] ActorPickupCoinSettings.Reference settings = new ActorPickupCoinSettings.Reference();
    [SerializeField] Transform m_modelRoot;

    public bool collected { get; private set; }

    Effect m_coinEffect;
    private GameObject m_coinModel;
    private GameObject m_collectedCoinModel;

    public Transform modelRoot { get { return m_modelRoot != null ? m_modelRoot : transform; }}

    protected virtual void Start()
    {
        m_coinModel = Instantiate(settings.value.coinModel, modelRoot);
        m_collectedCoinModel = Instantiate(settings.value.collectedCoinModel, modelRoot);
        SetCollected(false, true);
    }

    internal void TryToCollect()
    {
        if(collected)
        {
            return;
        }

        SetCollected(true);
    }

    private void SetCollected(bool v, bool force=false)
    {
        if(collected == v && !force)
        {
            return;
        }

        collected = v;

        if (m_coinModel != null) m_coinModel.SetActive(!collected);
        if (m_collectedCoinModel != null) m_collectedCoinModel.SetActive(collected);

        if(collected)
        {            

            Effect.Spawn(settings.value.pickupEffectPrefab, transform.position, transform.rotation);
            if(m_coinEffect)
            {
                m_coinEffect.Stop();
                m_coinEffect = null;
            }
        }
        else 
        {
            if (m_coinEffect==null)
            {
                m_coinEffect = Effect.Spawn(settings.value.coinEffectPrefab, transform);
            }
        }
    }
}
