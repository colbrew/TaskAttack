using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Projectile : MonoBehaviour {
    private static bool paused = false;
    public Rigidbody2D rb;
    public SpriteRenderer spriteRend;
    private WeaponType _type;
    Vector2 velocity;
    public AudioData projectileSound;
    public GameObject splashPrefab;
    public bool splash = false;

    public static bool Paused
    {
        get
        {
            return paused;
        }

        set
        {
            paused = value;
        }
    }

    public WeaponType Type
    {
        get
        {
            return _type;
        }

        set
        {
            SetType(value);
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRend = GetComponent<SpriteRenderer>();
        if(splash)
        {
            splashPrefab = Instantiate<GameObject>(splashPrefab);
        }
    }

    // Use this for initialization
    void Start () {
        velocity = rb.velocity;
    }

    private void Update()
    {
        if(Paused)
        {
            rb.velocity = Vector2.zero;
        }
        else{
            rb.velocity = velocity;
        }
        if(splash)
        {
            Vector3 splashPosition = this.transform.position;
            splashPosition.y = Camera.main.ViewportToWorldPoint(new Vector2(0, 0)).y;
            splashPrefab.transform.position = splashPosition;
            splashPrefab.transform.SetParent(this.transform, true);
        }
    }

    public void SetType ( WeaponType eType)
    {
        _type = eType;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && !Player.S.playerStarting)
        {
            Player.S.LoseLife();
            gameObject.SetActive(false);
        }
    }
}
