using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Cratesmith
{
    public class SensorTrigger<T> : Sensor<T> where T:Component
    {
        public bool detectTriggers = false;

	    private readonly HashSet<SensorItem> m_inSensor = new HashSet<SensorItem>();
        private readonly HashSet<T> m_objectsInSensor = new HashSet<T>();

        public override IEnumerable<SensorItem> sensorItems { get { return m_inSensor; } }
        public override IEnumerable<T> objectsInSensor { get { return m_objectsInSensor; } }
        public override int objectCount { get { return m_objectsInSensor.Count; } }

        protected virtual void Awake()
        {
            var trigger = gameObject.GetOrAddComponent<Collider, SphereCollider>();
            trigger.isTrigger = true;
        }          

        private T GetObjectFromCollider(Collider col)
        {
            return (!col.isTrigger||detectTriggers) ? col.GetComponentInParent<T>():null;
        }

        public override bool Contains(T obj)
        {
            return m_objectsInSensor.Contains(obj);
        }

        protected virtual void OnTriggerEnter(Collider col)
        {
            var obj = GetObjectFromCollider(col);
            if (obj && !m_objectsInSensor.Contains(obj))
		    {

                m_inSensor.Add(new SensorItem() { objectInSensor=obj, timeEntered=Time.time });
                m_objectsInSensor.Add(obj );
			    OnEnter(obj );
		    }
	    }

        protected virtual void OnTriggerExit(Collider col)
	    {
	        var obj = GetObjectFromCollider(col);
	        if (!obj)
	        {
	            return;
	        }

		    int numBefore = m_inSensor.Count;

		    m_inSensor.RemoveWhere(x=> obj == x.objectInSensor);
            m_objectsInSensor.RemoveWhere(x => obj == x);

            if (m_inSensor.Count != numBefore)
		    {
			    OnExit(obj);
		    }
	    }
    
        static readonly List<SensorItem> s_removeList = new List<SensorItem>();
        void Update()
        {
            var e = m_inSensor.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.objectInSensor==null)
                {
                    s_removeList.Add(e.Current);    
                }            
            }

            for (int i = 0; i < s_removeList.Count; i++)
            {
                m_inSensor.Remove(s_removeList[i]);

                var obj = s_removeList[i].objectInSensor;
                if (obj)
                {
                    m_objectsInSensor.Remove(obj);                
                }
            }
            s_removeList.Clear();
        }
    }
}
