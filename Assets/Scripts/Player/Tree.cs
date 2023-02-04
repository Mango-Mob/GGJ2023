using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public Transform centreOfMass;
    public GameObject branches;
    public Mesh onHitMesh;

    private Rigidbody rigidbody;
    [SerializeField] private float breakVelocity = 5.0f;
    [SerializeField] private float impactMult = 2000.0f;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.centerOfMass = centreOfMass.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("PlayerCar"))
        {
            if (rigidbody.isKinematic)
                collision.collider.GetComponentInParent<Car>().HaltWheels();

            if (collision.relativeVelocity.magnitude > breakVelocity)
            {
                Debug.Log(collision.relativeVelocity.magnitude);
                GetComponentInChildren<MeshFilter>().mesh = onHitMesh;
                branches.SetActive(false);
                rigidbody.isKinematic = false;
                rigidbody.AddForceAtPosition(collision.relativeVelocity * impactMult, collision.contacts[0].point);
                GetComponent<MultiAudioAgent>().PlayRandom();
            }
        }
    }
}
