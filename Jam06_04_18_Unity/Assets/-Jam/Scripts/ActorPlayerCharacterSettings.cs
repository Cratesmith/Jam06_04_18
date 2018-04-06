using UnityEngine;
using System;

public class ActorPlayerCharacterSettings : SettingsAsset<ActorPlayerCharacterSettings>
{
    public ActorPlayerCameraTarget cameraTargetPrefab;
    public Effect runEffectPrefab;

    [Serializable] public class Reference : SettingsReference<ActorPlayerCharacterSettings> {}
}