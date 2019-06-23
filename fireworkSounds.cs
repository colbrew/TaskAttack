using UnityEngine;

public class fireworkSounds : MonoBehaviour {
    public AudioData fireworkExplosion;
    int numberOfParticles = 0;
 
private void Update()
    {
       int count = GetComponent<ParticleSystem>().particleCount;
       if (count > numberOfParticles)
        { //particle has been born
            fireworkExplosion.Play(transform);
        }

        numberOfParticles = count;
    }
}
