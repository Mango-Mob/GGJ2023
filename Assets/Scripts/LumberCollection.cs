using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberCollection : MonoBehaviour
{
    public Animator animator;

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
            animator.SetTrigger("Cut");
            GetComponent<SoloAudioAgent>().Play();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("PlayerCar"))
        {
            Tree tree = other.GetComponent<Car>().GetHookedTree();
            if (tree)
            {
                Prop prop = tree.GetComponent<Prop>();
                prop?.owner.Collect(prop);
            }
            animator.SetTrigger("Cut");
            GetComponent<SoloAudioAgent>().Play();
        }
    }
}
