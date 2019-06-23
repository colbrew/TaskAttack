using UnityEngine;

public class BoundaryCheck : MonoBehaviour {

    public bool keepOnScreen = false;
    public float radius;
    Vector3 pos;
    public bool enteredFray = false;

    void Update()
    {
        if (keepOnScreen)
        {
            pos = Camera.main.WorldToViewportPoint(transform.position);
            pos.x = Mathf.Clamp01(pos.x);
            pos.y = Mathf.Clamp01(pos.y);
            transform.position = Camera.main.ViewportToWorldPoint(pos);
        }
        else{
            pos = Camera.main.WorldToViewportPoint(transform.position);

            if (pos.x > 0 && pos.x < 1 && pos.y > 0 && pos.y < 1)
            {
                enteredFray = true;
            }

            // test for projectiles or text message shards that have left the screen
            if (Offscreen() && (gameObject.tag == "PlayerProjectile" || gameObject.tag == "EnemyProjectile" || gameObject.tag == "TextShard"))
            {
                Destroy(gameObject);
            }

            // test for enemies who have left the screen
            if (enteredFray && Offscreen() && gameObject.tag == "Enemy")
            {
                gameObject.SetActive(false);
            }

        }
    }

    public bool Offscreen()
    {
        if(pos.x < 0 || pos.x > 1 || pos.y < 0 || pos.y > 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
