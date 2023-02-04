using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberCollection : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Wood")
        {
            if(!other.attachedRigidbody.isKinematic)
            {
                Prop prop = other.GetComponent<Prop>();
                prop?.owner.Collect(prop);

                if (!prop)
                    Destroy(other.gameObject);
            }
        }
    }
}
