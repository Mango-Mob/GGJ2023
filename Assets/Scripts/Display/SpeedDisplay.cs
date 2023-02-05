using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpeedDisplay : MonoBehaviour
{
    public float minAngle;
    public float maxAngle;

    [Range(0f, 1f)]
    public float value;

    [Range(0f, 1f)]
    public float lerp = 0.5f;
    private float actual_value;
    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.Euler(0, 0, minAngle);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(minAngle, maxAngle, actual_value));

        actual_value = Mathf.Lerp(actual_value, value, lerp);
    }
}
