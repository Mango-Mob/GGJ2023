using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    private Collider m_collider;

    // Start is called before the first frame update
    void Awake()
    {
        m_collider = GetComponent<Collider>();
    }

    public Vector3 RecalculateCameraLocation(Vector3 _pos)
    {
        return m_collider.ClosestPoint(_pos);
    }
}
