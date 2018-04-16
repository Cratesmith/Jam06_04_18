using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Cratesmith
{
    public abstract class Sensor<T> : MonoBehaviour where T : Component
    {
        [SerializeField] protected UnityEvent onEnterEvent;
        public event System.Action<T> onEnter;

        [SerializeField] protected UnityEvent onExitEvent;
        public event System.Action<T> onExit;

        [SerializeField] protected UnityEvent onFirstEnterEvent;
        [SerializeField] protected UnityEvent onLastExitEvent;

        public class SensorItem
        {
            public T objectInSensor;
            public float timeEntered;
        }

        public abstract IEnumerable<SensorItem> sensorItems { get; }
        public abstract IEnumerable<T> objectsInSensor { get; }

        public abstract int objectCount { get; }
        public abstract bool Contains(T obj);

        protected void OnEnter(T obj)
        {
            if (onEnterEvent != null)
            {
                onEnterEvent.Invoke();
            }

            if (onFirstEnterEvent != null && objectCount == 1)
            {
                onFirstEnterEvent.Invoke();
            }

            if (onEnter != null)
                onEnter(obj);
        }

        protected void OnExit(T obj)
        {
            if (onExitEvent != null)
            {
                onExitEvent.Invoke();
            }

            if (onLastExitEvent != null && objectCount > 0)
            {
                onLastExitEvent.Invoke();
            }

            if (onExit != null)
                onExit(obj);
        }
    }
}