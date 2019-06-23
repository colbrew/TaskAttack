using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{

    public BGScrollSpeed scrollSpeed;
    public float tileSizeZ;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (Player.S != null)
        {
            if (!Player.S.celebration && !Player.S.isTextInterlude)
            {
                float newPosition = Mathf.Repeat(Time.time * scrollSpeed.speed, tileSizeZ);
                transform.position = startPosition + Vector3.down * newPosition;
            }
        }
    }
}
