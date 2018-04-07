﻿using Cratesmith;
using UnityEngine;

public class PhysicsCharacterController : SubComponent<Actor>
{
    public PhysicsCharacterControllerSettings.Reference settings = new PhysicsCharacterControllerSettings.Reference();

    public bool isGrounded { get; private set; }
    public bool jump { get; set; }
    public Vector2 moveXZ { get; set; }

    private bool m_isJumpRising;
    private Vector3 m_prevPosition;
    private Rigidbody m_rigidBody;
    private CapsuleCollider m_capsuleCollider;
    private Vector3 m_groundNormal;
    private Vector3 m_velocity;
    private Vector3 m_desiredVelocity;

    protected void Awake()
    {
        m_rigidBody = gameObject.GetOrAddComponent<Rigidbody>();
        settings.value.rigidbodySettings.Apply(m_rigidBody);

        m_capsuleCollider = gameObject.GetOrAddComponent<CapsuleCollider>();
        settings.value.colliderSettings.Apply(m_capsuleCollider);
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
        var settings = this.settings.value;
        var desiredVelocity = settings.maxSpeed * moveXZ.X_Y() + Vector3.up * m_rigidBody.velocity.y;

        // check if the jump is still rising
        m_isJumpRising &= m_rigidBody.velocity.y > 0;

        // stepRay - find the ground
        var localStepRayOrigin = new Vector3(0, settings.colliderSettings.stepHeight, 0);
        var mainStepRay = new Ray(transform.TransformPoint(localStepRayOrigin), -transform.up);
        var stepRay = mainStepRay;
        RaycastHit raycastHit;
        var mainStepRayResult = Physics.Raycast(stepRay, out raycastHit, settings.colliderSettings.stepHeight * 2);
        if (!mainStepRayResult)
        {
            if (Physics.SphereCast(transform.TransformPoint(new Vector3(0, settings.colliderSettings.height, 0)),
                settings.colliderSettings.radius / 2f,
                -transform.up,
                out raycastHit,
                settings.colliderSettings.height))
            {
                var localHit = transform.InverseTransformPoint(raycastHit.point);
                stepRay.origin = transform.TransformPoint(new Vector3(localHit.x, localStepRayOrigin.y, localHit.z));
                mainStepRayResult = Physics.Raycast(stepRay, out raycastHit, settings.colliderSettings.stepHeight * 2);
            }

        }

        // if we found a ground. these are the variables for it
        var hitPointLocal = transform.InverseTransformPoint(raycastHit.point);
        var groundPoint = transform.TransformPoint(0, hitPointLocal.y, 0f);
        var withinStepHeight = raycastHit.distance - 0.001f <= settings.colliderSettings.stepHeight;
        var slopeAngle = Vector3.Angle(Vector3.up, raycastHit.normal);
        var walkableSlope = slopeAngle < settings.maxSlopeAngle;
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

        Debug.DrawRay(stepRay.origin, stepRay.direction * settings.colliderSettings.stepHeight * 2f,
            raycastHit.collider != null ? Color.green : Color.red);
       
        // apply desired vel       
        var desiredVelDotVelXZ = Vector2.Dot(m_rigidBody.velocity.XZ(), moveXZ);
        var blendRate = isGrounded ? (desiredVelDotVelXZ > 0 ? settings.onGroundVelocityBlend : settings.groundBreakVelocityBlend) : settings.airVelocityBlend;
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
        m_rigidBody.velocity += Physics.gravity * settings.gravityFactor * Time.fixedDeltaTime;

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
            m_rigidBody.velocity += Vector3.Lerp(m_groundNormal,transform.up,.5f) * settings.jumpImpulse;
            jump = false;
            m_isJumpRising = true;
            isGrounded = false;
        }

        m_prevPosition = transform.position;
        m_velocity = m_rigidBody.velocity;
        m_desiredVelocity = desiredVelocity;
    }
}