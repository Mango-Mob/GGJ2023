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
        else if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCar"))
        {
            Tree tree = other.GetComponent<Car>().GetHookedTree();
            if (tree)
            {
                Prop prop = tree.GetComponent<Prop>();
                prop?.owner.Collect(prop);
            }
        }
    }
}
