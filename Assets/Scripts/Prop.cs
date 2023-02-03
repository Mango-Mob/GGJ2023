using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{
    public PropFactory owner;

    private void OnDestroy()
    {
        if (owner)
            owner.Remove(this);
    }
}