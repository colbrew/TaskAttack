using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeColliderSetup : MonoBehaviour
{
    private void Awake()
    {
        AddEdges();
    }

    void AddEdges()
    {
        if (Camera.main == null) { Debug.LogError("Camera.main not found, failed to create edge colliders"); return; }

        var cam = Camera.main;
        if (!cam.orthographic) { Debug.LogError("Camera.main is not Orthographic, failed to create edge colliders"); return; }

        var width = Screen.width;
        var height = Screen.height;

        var bottomLeft = (Vector2)cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        var topLeft = (Vector2)cam.ScreenToWorldPoint(new Vector3(0, height, cam.nearClipPlane));
        var topMiddle = (Vector2)cam.ScreenToWorldPoint(new Vector3(width/ 2, (height * 1.02f), cam.nearClipPlane));
        var topRight = (Vector2)cam.ScreenToWorldPoint(new Vector3(width, height, cam.nearClipPlane));
        var bottomRight = (Vector2)cam.ScreenToWorldPoint(new Vector3(width, 0, cam.nearClipPlane));
        var bottomMiddle = (Vector2)cam.ScreenToWorldPoint(new Vector3(width / 2, -(height * .02f), cam.nearClipPlane));

        // add or use existing EdgeCollider2D
        var edge = GetComponent<EdgeCollider2D>() == null ? gameObject.AddComponent<EdgeCollider2D>() : GetComponent<EdgeCollider2D>();

        var edgePoints = new[] { bottomLeft, topLeft, topMiddle, topRight, bottomRight, bottomMiddle, bottomLeft };

        edge.points = edgePoints;
    }
}
