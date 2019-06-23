using UnityEngine;

public class StarfieldMovement : MonoBehaviour {
    MeshRenderer mr;
    Material mat;
    public float parallaxScale = 1;

    // Use this for initialization
    void Start () {
        mr = GetComponent<MeshRenderer>();
        mat = mr.material;
    }
    
    // Update is called once per frame
    void Update () {
        Vector2 offset = mat.mainTextureOffset;
        offset.x = (Player.S.transform.position.x / 100) * parallaxScale;
        offset.y = (Player.S.transform.position.y / 100) * parallaxScale;

        mat.mainTextureOffset = offset;
    }
}