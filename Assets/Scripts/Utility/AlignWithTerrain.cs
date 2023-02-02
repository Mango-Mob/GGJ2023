using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignWithTerrain : MonoBehaviour
{
    RaycastHit hit;
    Vector3 theRay;

    public LayerMask terainMask;
    public bool m_instant = false;

    void FixedUpdate()
    {
        Align();
    }

    private void Align()
    {
        theRay = -transform.up;

        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y, transform.position.z),
            theRay, out hit, 20, terainMask))
        {

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.parent.rotation;

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, (m_instant ? 1.0f : Time.deltaTime / 0.15f));
        }
    }
}
