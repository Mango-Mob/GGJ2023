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
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.m_player.GetComponent<Car>().NotifyDestroyed(this);
    }
    public void Uproot()
    {
        if (!rigidbody.isKinematic)
            return;

        GetComponentInChildren<MeshFilter>().mesh = onHitMesh;
        branches.SetActive(false);
        rigidbody.isKinematic = false;
        GetComponent<MultiAudioAgent>().PlayRandom();
        var particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var item in particles)
        {
            item.Play();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("PlayerCar"))
        {
            Car car = collision.collider.GetComponentInParent<Car>();

            if (!car)
                return;

            if (rigidbody.isKinematic)
                car.HaltWheels();

            if (collision.relativeVelocity.magnitude * car.impactMult > breakVelocity && rigidbody.isKinematic)
            {
                rigidbody.AddForceAtPosition(collision.relativeVelocity * impactMult * car.impactMult, collision.contacts[0].point);
                Uproot();
            }
        }
    }
}
