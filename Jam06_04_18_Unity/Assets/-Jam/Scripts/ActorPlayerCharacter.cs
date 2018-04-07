
using Cratesmith;
using UnityEngine;

public class ActorPlayerCharacter : Actor
{
    public ActorPlayerCharacterSettings.Reference settings = new ActorPlayerCharacterSettings.Reference();
    public PhysicsCharacterController characterController { get; private set;}
    private ActorPlayerCameraTarget m_cameraTarget;

    private float m_prevSpeed = 0;
    private Effect m_runEffect;
    private Effect m_runEffectPrefab;

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
            if (Input.GetButton("Fire1")) 
            {
                input2d *= 0.5f;   
            }
            var input = input2d.x*camTranform.right + input2d.y*camTranform.forward;           
            var inputXZ = input.XZ().normalized * Mathf.Min(input2d.magnitude,1f);

            characterController.moveXZ = inputXZ;
            characterController.jump = Input.GetButton("Jump");
        }

        Effect newEffectPrefab = null;
        if (characterController.isGrounded)
        {
            var speed = characterController.velocity.magnitude;
            foreach (var runEffect in settings.value.runEffects)
            {
                if (speed >= runEffect.minSpeed)
                {
                    newEffectPrefab = runEffect.effectPrefab;
                }
            }
        }

        if(newEffectPrefab!=m_runEffectPrefab)
        {
            m_runEffectPrefab = newEffectPrefab;
            if(m_runEffect!=null)
            {
                m_runEffect.Stop();
                m_runEffect = null;
            }

            if(m_runEffectPrefab!=null)
            {
                m_runEffect = Effect.Spawn(m_runEffectPrefab, transform.position, transform.rotation, transform);
            }
        }
    }
}
