using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public Transform centreOfMass;
    public GameObject branches;
    public Mesh onHitMesh;

    public ParticleSystem treePoof;
    public ParticleSystem trunkPoof;

    private Rigidbody rigidbody;
    [SerializeField] private float breakVelocity = 5.0f;
    [SerializeField] private float impactMult = 2000.0f;

    private float delay = 0.0f;
    private bool is_detatching = false;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.centerOfMass = centreOfMass.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (delay > 0)
            delay -= Time.deltaTime;
        else if (is_detatching)
            DetachVFX( 0 );
    }
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.m_player.GetComponent<Car>().NotifyDestroyed(this);
        
    }

    public void DetachVFX(float _delay)
    {
        if(!is_detatching)
        {
            is_detatching = true;
            delay = _delay;
            return;
        }
        trunkPoof.gameObject.SetActive(true);
        trunkPoof.Play();
        trunkPoof.transform.SetParent(null);
    }

    public void Uproot()
    {
        if (!rigidbody.isKinematic)
            return;

        GetComponentInChildren<MeshFilter>().mesh = onHitMesh;
        branches.SetActive(false);
        rigidbody.isKinematic = false;
        GetComponent<MultiAudioAgent>().PlayRandom( false );
        var particles = treePoof.GetComponentsInChildren<ParticleSystem>();
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
