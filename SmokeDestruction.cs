using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeDestruction : MonoBehaviour {

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log("Particle Collision");
        Player.S.LoseLife();
    }
}
