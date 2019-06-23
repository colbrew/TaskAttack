using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PutSparksInACorner : MonoBehaviour
{
    public enum Corner
    {
        bottomLeft,
        topLeft,
        topRight,
        bottromRight
    }

    [Header("Set in Inspector")]
    [Tooltip("Select which corner you want the sparks to be positioned at.")]
    public Corner corner;

    private void Awake()
    {
        Camera cam = Camera.main;

        switch(corner)
        {
            case (Corner.bottomLeft):
                transform.position = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
                break;
            
            case (Corner.topLeft):
                transform.position = cam.ScreenToWorldPoint(new Vector3(0, Screen.height, cam.nearClipPlane));
                break;

            case (Corner.topRight):
                transform.position = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.nearClipPlane));
                break;

            case (Corner.bottromRight):
                transform.position = cam.ScreenToWorldPoint(new Vector3(Screen.width, 0, cam.nearClipPlane));
                break;

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
