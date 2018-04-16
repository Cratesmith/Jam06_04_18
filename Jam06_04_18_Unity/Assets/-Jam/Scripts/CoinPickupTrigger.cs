using System.Collections;
using System.Collections.Generic;
using Cratesmith;
using UnityEngine;

[RequireComponent(typeof(SensorTriggerPlayerCharacter))]
public class CoinPickupTrigger : SubComponent<ActorPickupCoin>
{
    private SensorTriggerPlayerCharacter m_sensor;

    protected virtual void Awake()
    {
        m_sensor = gameObject.GetOrAddComponent<SensorTriggerPlayerCharacter>();
    }

    protected virtual void OnEnable()
    {
        m_sensor.onEnter += M_Sensor_OnEnter;
    }

    protected virtual void OnDisable()
    {
        if(m_sensor)
        {
            m_sensor.onEnter -= M_Sensor_OnEnter;
        }
    }


    void M_Sensor_OnEnter(ActorPlayerCharacter obj)
    {
        owner.TryToCollect();
    }
}
