using System;
using UnityEngine;

public class ActorPickupCoinSettings : SettingsAsset<ActorPickupCoinSettings>
{
    [Serializable] public class Reference : SettingsReference<ActorPickupCoinSettings> { }

    public GameObject coinModel;
    public GameObject collectedCoinModel;

    public Effect coinEffectPrefab;
    public Effect pickupEffectPrefab;
}
