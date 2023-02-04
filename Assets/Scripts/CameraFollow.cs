using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject target;

    // Update is called once per frame
    void LateUpdate()
    {
        if(target)
        {
            this.transform.position = new Vector3(
                target.transform.position.x,
                transform.position.y,
                target.transform.position.z
                );
        }
    }
}
