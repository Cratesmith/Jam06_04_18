using System;
using UnityEngine;

public class PhysicsCharacterControllerSettings : SettingsAsset<PhysicsCharacterControllerSettings>
{
    public AnimationCurve groundAccelerationVsSpeed = new AnimationCurve();
    public AnimationCurve breakAccelerationVsSpeed = new AnimationCurve();
    public AnimationCurve airAccelerationVsSpeed = new AnimationCurve();

    public float onGroundVelocityBlend = 25;
    public float groundBreakVelocityBlend = 30;
    public float airVelocityBlend = 5;
    public float jumpImpulse = 10;
    public float earlyJumpTime = .2f;
    public float lateJumpTime = .2f;
    public float gravityFactor = 2;
    public float maxSlopeAngle = 45;
    public float maxSpeed = 10;

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

    public ColliderSettings colliderSettings = new ColliderSettings();

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
    public RigidbodySettings rigidbodySettings = new RigidbodySettings();

    [Serializable]
    public class Reference : SettingsReference<PhysicsCharacterControllerSettings>
    {
    }
}