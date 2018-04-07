using UnityEngine;
using System;

public class ActorPlayerCharacterSettings : SettingsAsset<ActorPlayerCharacterSettings>
{
    public ActorPlayerCameraTarget cameraTargetPrefab;


    [Serializable]
    public class RunEffect
    {
        public float    minSpeed = 0.5f;
        public Effect   effectPrefab;
    }
    public RunEffect[] runEffects = new RunEffect[0];

    [Serializable] public class Reference : SettingsReference<ActorPlayerCharacterSettings> {}
}