using Cinemachine;
using Cratesmith;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class ActorPlayerCameraTarget : Actor
{
    [SerializeField] float lookaheadTime = 0.5f; 
    [SerializeField] float lookaheadBlend = 2f;
    [SerializeField] float cameraHeightBlend = 2f;

    private Transform m_cameraTarget;
    private Vector3 m_Offset;
    private Vector3 m_transposerOffset;

    private float m_cameraHeight;

    public CinemachineVirtualCamera playerCamera { get; private set; }
    public ActorPlayerCharacter character { get; private set; }


    void Awake()
    {
        playerCamera = gameObject.GetOrAddComponent<CinemachineVirtualCamera>();
    }

    public void Init(ActorPlayerCharacter actorPlayerCharacter)
    {
        character = actorPlayerCharacter;

       
        var composer = playerCamera.GetCinemachineComponent<CinemachineComposer>();
        if (composer)
        {
            playerCamera.Follow = actorPlayerCharacter.transform;
            playerCamera.LookAt = actorPlayerCharacter.transform;
        }
        else 
        {
            var ctGO =  new GameObject("cameraTarget");;
            m_cameraTarget = ctGO.transform;
            m_cameraTarget.transform.parent = transform;
            playerCamera.Follow = m_cameraTarget.transform;
        }

    }

    void Update()
    {
        var composer = playerCamera.GetCinemachineComponent<CinemachineComposer>();
        if(composer)
        {
            composer.m_TrackedObjectOffset = Vector3.Lerp(composer.m_TrackedObjectOffset, character.transform.InverseTransformVector(character.characterController.velocity) * lookaheadTime, Time.deltaTime*lookaheadBlend);
        }
        else 
        {
            if(character.characterController.isGrounded)
            {
                m_cameraHeight = Mathf.Lerp(m_cameraHeight, character.transform.position.y, cameraHeightBlend * Time.deltaTime);
            }
            m_Offset = Vector3.Lerp(m_Offset, character.characterController.velocity.X_Z() * lookaheadTime, Time.deltaTime * lookaheadBlend);
            m_cameraTarget.position = character.transform.position.X_Z() + m_Offset + m_cameraHeight*Vector3.up;

        }
    }
}