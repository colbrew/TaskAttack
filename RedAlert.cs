using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedAlert : MonoBehaviour {

    float flashTime;
    public float flashSpeed = 2;
    Color color;
    SpriteRenderer spriteRend;

    // Use this for initialization
    void Start () {
        spriteRend = GetComponent<SpriteRenderer>();
    }
    
    // Update is called once per frame
    void Update () {
        // make player flash
        flashTime += Time.deltaTime;
        float u = Mathf.Abs(Mathf.Sin(flashTime * flashSpeed));
        color = spriteRend.color;
        color.a = Mathf.Lerp(0, .5f, u);
        spriteRend.color = color;
    }
}
