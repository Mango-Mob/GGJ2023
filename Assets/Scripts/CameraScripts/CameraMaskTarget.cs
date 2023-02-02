using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMaskTarget : MonoBehaviour
{
    static private CameraXray cameraXray;
    private MeshRenderer m_meshRenderer;
    private MeshRenderer m_phantomMeshRenderer;

    private Material m_storedMaterial;
    private Material m_phantomMaterial;
    // Start is called before the first frame update
    void Start()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();
        if (m_meshRenderer == null)
            Destroy(gameObject);

        if (cameraXray == null)
            cameraXray = GameManager.Instance.m_player.GetComponentInChildren<CameraXray>();

        cameraXray.SubscribeObject(this);

        m_storedMaterial = m_meshRenderer.material;
        m_phantomMaterial = new Material(cameraXray.m_phantomMaterial);

        m_phantomMaterial.mainTexture = m_storedMaterial.mainTexture;

        GameObject newObject = new GameObject("Phantom");
        newObject.transform.SetParent(transform);
        newObject.transform.localPosition = Vector3.zero;
        newObject.transform.localRotation = Quaternion.identity;
        newObject.transform.localScale = Vector3.one;
        newObject.AddComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
        
        m_phantomMeshRenderer = newObject.AddComponent<MeshRenderer>();
        m_phantomMeshRenderer.material = m_phantomMaterial;

        m_phantomMeshRenderer.enabled = false;
    }

    public void TogglePhantomMode(bool _active)
    {
        //m_meshRenderer.enabled = !_active;
        if (_active)
        {
            //m_meshRenderer.material = m_storedMaterial;
            m_phantomMeshRenderer.enabled = true;
            m_meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
        else
        {
            //m_meshRenderer.material = m_phantomMaterial;
            m_phantomMeshRenderer.enabled = false;
            m_meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }
}
