using System.Collections;
using System.Collections.Generic;
using Cratesmith;
using UnityEngine;

public class PlayerCharacterModel : SubComponent<ActorPlayerCharacter> 
{
    private static readonly int FLOAT_SPEED = Animator.StringToHash("Speed");
    private static readonly int FLOAT_AIRTIME = Animator.StringToHash("AirTime");
    private static readonly int TRIGGER_JUMP = Animator.StringToHash("Jump");
    private static readonly int BOOL_ISGROUNDED = Animator.StringToHash("IsGrounded");
    private static readonly int BOOL_ISSKIDDING = Animator.StringToHash("IsSkidding");
    
    private Animator m_animator;
    private bool m_wasJumping;

    void Awake()
    {
        m_animator = gameObject.GetOrAddComponent<Animator>();        
    }

    void Update()
    {        
        m_animator.SetFloat(FLOAT_SPEED, owner.characterController.moveXZ.magnitude);

        m_animator.SetFloat(FLOAT_AIRTIME, owner.characterController.airTime);

        m_animator.SetBool(BOOL_ISGROUNDED, owner.characterController.isGrounded);

        if(owner.characterController.isJumpRising && !m_wasJumping)
        {
            m_animator.SetTrigger(TRIGGER_JUMP);
        }
        m_wasJumping = owner.characterController.isJumpRising;

        m_animator.SetBool(BOOL_ISSKIDDING, owner.characterController.moveXZ.sqrMagnitude > 0f && Vector3.Dot(owner.characterController.velocity, owner.characterController.moveXZ.X_Y()) < 0);
    }
}
