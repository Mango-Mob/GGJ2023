using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberCollection : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Wood")
        {
            Prop prop = other.GetComponent<Prop>();
            prop?.owner.Collect(prop);
            Destroy(other.gameObject);
        }
    }
}
