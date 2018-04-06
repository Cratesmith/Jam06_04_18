using Cinemachine;
using Cratesmith;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class ActorPlayerCameraTarget : Actor
{
    public CinemachineVirtualCamera playerCamera { get; private set; }

    void Awake()
    {
        playerCamera = gameObject.GetOrAddComponent<CinemachineVirtualCamera>();
    }

    public void Init(ActorPlayerCharacter actorPlayerCharacter)
    {
        playerCamera.Follow = actorPlayerCharacter.transform;
        //playerCamera.LookAt = actorPlayerCharacter.transform;
    }
}