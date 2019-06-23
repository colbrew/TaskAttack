using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyExplosionAnim : MonoBehaviour {

    public float explosionTime;

    // Use this for initialization
    void Start () {
        Destroy(gameObject, explosionTime);
    }

}
