using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraXray : MonoBehaviour
{

    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject m_maskSphere;

    [SerializeField] private float m_detectRadius = 0.5f;
    [SerializeField] private float m_blockRange = 0.0f;
    [SerializeField] private LayerMask m_affectedLayers;
    private List<MeshRenderer> m_hiddenRenderers = new List<MeshRenderer>();
    private List<CameraMaskTarget> m_subscribedRenderers = new List<CameraMaskTarget>();

    public Material m_phantomMaterial;
    //private Dictionary<MeshRenderer, MeshRenderer> m_phantomRenderers;

    struct PhantomRenderer
    {
        MeshRenderer originalRenderer;
        MeshRenderer tempRenderer;
    }

    // Start is called before the first frame update
    void Start()
    {


    }

    public void SubscribeObject(CameraMaskTarget _cameraMaskTarget)
    {
        m_subscribedRenderers.Add(_cameraMaskTarget);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        List<CameraMaskTarget> affectedMeshes = new List<CameraMaskTarget>();

        Collider[] colliders = Physics.OverlapCapsule(transform.position, playerObject.transform.position - transform.forward * m_blockRange, m_detectRadius, m_affectedLayers);
        foreach (var collider in colliders)
        {
            CameraMaskTarget[] meshRenderers = collider.gameObject.GetComponentsInChildren<CameraMaskTarget>();

            foreach (var renderer in meshRenderers)
            {
                renderer.TogglePhantomMode(true);
                affectedMeshes.Add(renderer);
            }
        }

        foreach (var renderer in m_subscribedRenderers)
        {
            if (!affectedMeshes.Contains(renderer))
            {
                renderer.TogglePhantomMode(false);
            }
        }
                //CameraMaskTarget[] meshRenderers = collider.gameObject.GetComponentsInChildren<CameraMaskTarget>();

                //foreach (var renderer in meshRenderers)
                //{
                //    affectedMeshes.Add(renderer);
                //    if (!m_hiddenRenderers.Contains(renderer))
                //    {
                //        if (renderer.enabled)
                //        {
                //            m_hiddenRenderers.Add(renderer);
                //            renderer.enabled = false;
                //        }
                //    }

                //if (renderer.GetComponent<CameraMaskTarget>() != null)
                //    continue;

                //renderer.gameObject.AddComponent<CameraMaskTarget>();


                //if (m_phantomRenderers.ContainsKey(renderer))
                //    continue;

                //Vector3 objectPosition = hit.collider.gameObject.transform.position;
                //Vector3 objectScale = hit.collider.gameObject.transform.lossyScale;
                //Quaternion objectRotation = hit.collider.gameObject.transform.rotation;

                //GameObject newPhantom = new GameObject();

                //MeshRenderer newPhantomRenderer = newPhantom.AddComponent<MeshRenderer>();
                //newPhantom.transform.position = objectPosition;
                //newPhantom.transform.localScale = objectScale;
                //newPhantom.transform.rotation = objectRotation;

                //newPhantomRenderer.material = m_phantomMaterial;


        //List<MeshRenderer> removeList = new List<MeshRenderer>();
        //foreach (var renderer in m_hiddenRenderers)
        //{
        //    if (!affectedMeshes.Contains(renderer))
        //    {
        //        removeList.Add(renderer);
        //        renderer.enabled = true;
        //    }
        //}

        //foreach (var renderer in removeList)
        //{
        //    m_hiddenRenderers.Remove(renderer);
        //}
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position, m_detectRadius);
        Gizmos.DrawSphere(playerObject.transform.position - transform.forward * m_blockRange, m_detectRadius);
    }
}
