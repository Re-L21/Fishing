using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bobber : MonoBehaviour
{
    public event System.Action<Collision> OnBobberCollided;

    private void OnCollisionEnter(Collision collision)
    {
        OnBobberCollided?.Invoke(collision);
    }

}
