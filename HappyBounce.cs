using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HappyBounce : MonoBehaviour {

    float startTime;
    Vector3 startPosition;
    float speed = 5;

    // Use this for initialization
    void Start () {
        startTime = Time.time;
        startPosition = transform.position;
    }
    
    // Update is called once per frame
    void Update () {
        float u = Mathf.Abs(Mathf.Sin((Time.time - startTime) * speed));
        Vector3 position = startPosition;
        position.y += u;
        transform.position = position;
    }
}
