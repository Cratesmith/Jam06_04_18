﻿
using Cratesmith;
using UnityEngine;

public class ActorPlayerCharacter : Actor
{
    public ActorPlayerCharacterSettings.Reference settings = new ActorPlayerCharacterSettings.Reference();
    public PhysicsCharacterController characterController { get; private set;}
    private ActorPlayerCameraTarget m_cameraTarget;
    private Effect m_runEffect;

    void Awake()
    {
        characterController = gameObject.GetOrAddComponent<PhysicsCharacterController>();
        
        m_cameraTarget = Instantiate(settings.value.cameraTargetPrefab);
        if (m_cameraTarget != null)
        {
            m_cameraTarget.Init(this);
        }
    }

    void FixedUpdate()
    {
        if (m_cameraTarget != null)
        {
            var camTranform = m_cameraTarget.transform;
            var input2d = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            var input = input2d.x*camTranform.right + input2d.y*camTranform.forward;           
            var inputXZ = input.XZ().normalized * input2d.magnitude;

            characterController.moveXZ = inputXZ;
            characterController.jump = Input.GetButtonDown("Fire1");
        }

        if (characterController.isGrounded && characterController.moveXZ.sqrMagnitude > 0)
        {
            if (m_runEffect == null)
            {
                m_runEffect = Effect.Spawn(settings.value.runEffectPrefab, transform.position, transform.rotation, transform);
            }
        }
        else
        {
            if (m_runEffect != null)
            {
                m_runEffect.Stop();
                m_runEffect = null;
            }
        }
    }
}