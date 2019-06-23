using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyWhenParticlesDone : MonoBehaviour {

    private void Start()
    {
        Destroy(gameObject, GetComponent<ParticleSystem>().main.duration);
    }
}
