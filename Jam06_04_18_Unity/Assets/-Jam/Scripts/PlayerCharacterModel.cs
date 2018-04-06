using System.Collections;
using System.Collections.Generic;
using Cratesmith;
using UnityEngine;

public class PlayerCharacterModel : SubComponent<ActorPlayerCharacter> 
{
    private static readonly int ANIM_SPEED = Animator.StringToHash("Speed");
    
    private Animator m_animator;

    void Awake()
    {
        m_animator = gameObject.GetOrAddComponent<Animator>();        
    }

    void Update()
    {        
        m_animator.SetFloat(ANIM_SPEED, owner.characterController.moveXZ.magnitude);
    }
}
