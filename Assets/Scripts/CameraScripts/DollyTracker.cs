using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DollyTracker : MonoBehaviour
{
    public CinemachineVirtualCamera m_myCamera;
    public CinemachineSmoothPath m_myTrack;
    private GameObject m_player = null;

    public float x;
    public float y;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(m_player != null)
        {

            Quaternion rotation = m_myTrack.transform.rotation;
            Vector3 A = rotation * (m_myTrack.m_Waypoints[0].position) + m_myTrack.transform.position;
            Vector3 B = rotation * (m_myTrack.m_Waypoints[1].position) + m_myTrack.transform.position;

            Vector3 AP = m_player.transform.position - A;
            Vector3 AB = B - A;

            float t = (Vector3.Dot(AP, AB) / Vector3.Dot(AB, AB));

            m_myCamera.GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition = t;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            m_player = other.gameObject;

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            m_player = null;
            m_myCamera.Priority /= 10;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Quaternion rotation = m_myTrack.transform.rotation;
        Vector3 A = rotation * (m_myTrack.m_Waypoints[0].position) + m_myTrack.transform.position;
        Vector3 B = rotation * (m_myTrack.m_Waypoints[1].position) + m_myTrack.transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(A, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(B, 0.5f);
    }
}
