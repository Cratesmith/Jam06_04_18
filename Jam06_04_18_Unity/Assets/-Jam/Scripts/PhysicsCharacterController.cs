using Cratesmith;
using UnityEngine;

public class PhysicsCharacterController : SubComponent<Actor>
{
    [SerializeField] private float m_onGroundVelocityBlend = 25;
    [SerializeField] private float m_groundBreakVelocityBlend = 30;
    [SerializeField] private float m_airVelocityBlend = 5;
    [SerializeField] private float m_jumpImpulse = 10;
    [SerializeField] private float m_gravityFactor = 2;
    [SerializeField] private float m_maxSlopeAngle = 45;
    [SerializeField] private float m_maxSpeed = 10;

    public bool isGrounded { get; private set; }
    public bool jump { get; set; }
    public Vector2 moveXZ { get; set; }

    private bool m_isJumpRising;

    [System.Serializable]
    public class ColliderSettings
    {
        public float height;
        public float radius;
        public float stepHeight = .25f;

        private static PhysicMaterial s_physMaterial;

        public void Apply(CapsuleCollider capsuleCollider)
        {
            capsuleCollider.radius = radius;
            capsuleCollider.height = height - stepHeight;
            capsuleCollider.center = new Vector3(0, capsuleCollider.height / 2f + stepHeight, 0);
            if (s_physMaterial == null)
            {
                s_physMaterial = new PhysicMaterial("PhysicsCharacter");
                s_physMaterial.dynamicFriction = 0;
                s_physMaterial.staticFriction = 0;
                s_physMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
                s_physMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
                s_physMaterial.bounciness = 0;
            }
            capsuleCollider.sharedMaterial = s_physMaterial;
        }
    }

    [SerializeField] ColliderSettings m_colliderSettings = new ColliderSettings();


    [System.Serializable]
    public class RigidbodySettings
    {
        public float mass = 1f;

        public void Apply(Rigidbody rigidBody)
        {
            rigidBody.mass = mass;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            rigidBody.useGravity = false;
        }
    }

    [SerializeField] private RigidbodySettings m_rigidbodySettings = new RigidbodySettings();

    private Vector3 m_prevPosition;
    private Rigidbody m_rigidBody;
    private CapsuleCollider m_capsuleCollider;
    private Vector3 m_groundNormal;
    private Vector3 m_velocity;
    private Vector3 m_desiredVelocity;

    protected void Awake()
    {
        m_rigidBody = gameObject.GetOrAddComponent<Rigidbody>();
        m_rigidbodySettings.Apply(m_rigidBody);

        m_capsuleCollider = gameObject.GetOrAddComponent<CapsuleCollider>();
        m_colliderSettings.Apply(m_capsuleCollider);
    }

    void Start()
    {
        m_prevPosition = transform.position;
    }


    void FixedUpdate()
    {
        FixedUpdate_Move();
        
        if (moveXZ.sqrMagnitude > 0f)
        {
            var currentAngle = transform.forward.XZ().ToAngle();
            var desiredAngle = moveXZ.ToAngle();
            //Debug.LogFormat("{0}: FixedUpdate currentAngle={1}, desiredAngle={2}",this,currentAngle,desiredAngle);
            transform.rotation = Quaternion.AngleAxis(90-Mathf.LerpAngle(currentAngle,desiredAngle, 8f*Time.fixedDeltaTime), Vector3.up);            
        }
    }

    private void FixedUpdate_Move()
    {
        var desiredVelocity = m_maxSpeed * moveXZ.X_Y() + Vector3.up * m_rigidBody.velocity.y;

        // check if the jump is still rising
        m_isJumpRising &= m_rigidBody.velocity.y > 0;

        // stepRay - find the ground
        var localStepRayOrigin = new Vector3(0, m_colliderSettings.stepHeight, 0);
        var mainStepRay = new Ray(transform.TransformPoint(localStepRayOrigin), -transform.up);
        var stepRay = mainStepRay;
        RaycastHit raycastHit;
        var mainStepRayResult = Physics.Raycast(stepRay, out raycastHit, m_colliderSettings.stepHeight * 2);
        if (!mainStepRayResult)
        {
            if (Physics.SphereCast(transform.TransformPoint(new Vector3(0, m_colliderSettings.height, 0)),
                m_colliderSettings.radius / 2f,
                -transform.up,
                out raycastHit,
                m_colliderSettings.height))
            {
                var localHit = transform.InverseTransformPoint(raycastHit.point);
                stepRay.origin = transform.TransformPoint(new Vector3(localHit.x, localStepRayOrigin.y, localHit.z));
                mainStepRayResult = Physics.Raycast(stepRay, out raycastHit, m_colliderSettings.stepHeight * 2);
            }

        }

        // if we found a ground. these are the variables for it
        var hitPointLocal = transform.InverseTransformPoint(raycastHit.point);
        var groundPoint = transform.TransformPoint(0, hitPointLocal.y, 0f);
        var withinStepHeight = raycastHit.distance - 0.001f <= m_colliderSettings.stepHeight;
        var slopeAngle = Vector3.Angle(Vector3.up, raycastHit.normal);
        var walkableSlope = slopeAngle < m_maxSlopeAngle;
        var flatNormal = new Vector3(raycastHit.normal.x, 0, raycastHit.normal.z).normalized;

        // are we on the ground?
        if (mainStepRayResult)
        {
            isGrounded = withinStepHeight && !m_isJumpRising;
            if (isGrounded)
            {
                m_groundNormal = raycastHit.normal;                
            }                  
        }
        else
        {
            isGrounded = false;
        }

        Debug.DrawRay(stepRay.origin, stepRay.direction * m_colliderSettings.stepHeight * 2f,
            raycastHit.collider != null ? Color.green : Color.red);
       
        // apply desired vel       
        var desiredVelDotVelXZ = Vector2.Dot(m_rigidBody.velocity.XZ(), moveXZ);
        var blendRate = isGrounded ? (desiredVelDotVelXZ > 0 ? m_onGroundVelocityBlend : m_groundBreakVelocityBlend) : m_airVelocityBlend;
        var desiredDotFlatNormal = Vector3.Dot(flatNormal, desiredVelocity);
        if (!walkableSlope)
        {
            if(desiredDotFlatNormal < 0)
            {
                desiredVelocity += -flatNormal * desiredDotFlatNormal;
            }
        }
        desiredVelocity.y = m_rigidBody.velocity.y;
        m_rigidBody.velocity = Vector3.Lerp(m_rigidBody.velocity, desiredVelocity, Time.fixedDeltaTime * blendRate);

        // gravity
        m_rigidBody.velocity += Physics.gravity * m_gravityFactor * Time.fixedDeltaTime;

        // stick us to the ground. modify our velocity if groundray has run into the ground
        if (isGrounded)
        {
            m_rigidBody.MovePosition(groundPoint);

            if (Vector3.Dot(raycastHit.normal, m_rigidBody.velocity) <= 0)
            {             
                m_rigidBody.velocity += -raycastHit.normal * Vector3.Dot(raycastHit.normal, m_rigidBody.velocity);
            }
            Debug.DrawRay(raycastHit.point, transform.forward, Color.green);
        }

        // handle jump if we're still grounded
        if (jump && isGrounded)
        {            
            m_rigidBody.velocity += Vector3.Lerp(m_groundNormal,transform.up,.5f) * m_jumpImpulse;
            jump = false;
            m_isJumpRising = true;
            isGrounded = false;
        }

        m_prevPosition = transform.position;
        m_velocity = m_rigidBody.velocity;
        m_desiredVelocity = desiredVelocity;
    }
}