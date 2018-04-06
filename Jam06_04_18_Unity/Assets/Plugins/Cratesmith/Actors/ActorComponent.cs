using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubComponent<T> : MonoBehaviour 
{
    private T m_owner;
    public T owner 
    {
        get 
        {
            if(m_owner==null)
            {
                m_owner = GetComponentInParent<T>();
            }
            return m_owner;
        }
    }
}
