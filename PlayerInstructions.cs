using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInstructions : MonoBehaviour {

    public GameObject projectilePrefab;
    public float shotDelay = .4f;
    float lastShotTime = 0;
    GameObject projectile;
    float projectileVelocity = 10;

    // Use this for initialization
    void Start () {
        
    }
    
    // Update is called once per frame
    void Update () {
        if (Time.time - lastShotTime > shotDelay)
        {
            projectile = Instantiate<GameObject>(projectilePrefab);
            projectile.transform.position = this.transform.position;
            projectile.GetComponent<Rigidbody2D>().velocity = Vector3.up * projectileVelocity;
            lastShotTime = Time.time;
        }
    }
}
