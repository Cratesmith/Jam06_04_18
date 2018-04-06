using UnityEngine;

public abstract class EffectComponent : SubComponent<Effect> 
{
    // is this sub effect still going? 
    public abstract bool isPlaying { get; }

    // stop this effect
    public abstract void Stop();

    // quick lookup for parent
    public Transform parent {get { return owner.parent; } }

    void OnEnable()
    {
        owner.AddEffectComponent(this);
    }

    void OnDisable()
    {
        owner.RemoveEffectComponent(this);
    }
}