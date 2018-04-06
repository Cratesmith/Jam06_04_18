using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cratesmith;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CinemachineBrain))]
public class ActorCamera : Actor 
{
    public new Camera camera { get; private set; }
    public CinemachineBrain cinemachineBrain { get; private set; }

    void Awake()
    {
        camera = gameObject.GetOrAddComponent<Camera>();
        cinemachineBrain = gameObject.GetOrAddComponent<CinemachineBrain>();
    }
}
