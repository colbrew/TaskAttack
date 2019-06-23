using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineFlash : MonoBehaviour {

    SpriteRenderer spriteRend;
    float flashTime = 0;
    public float flashSpeed = 10;
    Color rendColor;
    public float flashDuration = 3;

	// Use this for initialization
	void Start () {
        spriteRend = GetComponent<SpriteRenderer>();
        rendColor = spriteRend.color; 
	}
	
	// Update is called once per frame
	void Update () {
        if (flashTime < flashDuration)
        {
            flashTime += Time.deltaTime;
            float u = Mathf.Abs(Mathf.Sin(flashTime * flashSpeed));
            rendColor.a = Mathf.Lerp(0, 1, u);
            spriteRend.color = rendColor;
        }
        else
        {
            Destroy(gameObject);
        }
	}
}
