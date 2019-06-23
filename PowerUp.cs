using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour {
    [Header("Set in Inspector")]
    public Vector2 driftMinMax = new Vector2(.25f, 1);
    public float lifeTime = 4f;
    public float fadeTime = 3f;
    public AudioData absorbSound;
    public float flashSpeed = 10;

    [Header("Set Dynamically")]
    public WeaponType type;
    public float birthTime;
    public GameObject powerUp;
    private BoundaryCheck boundaryCheck;
    private SpriteRenderer spriteRend;
    Rigidbody2D rb2d;
    float flashTime;
    Color color;




    // Use this for initialization
    void Awake () {
        spriteRend = GetComponent<SpriteRenderer>();
        color = spriteRend.color;
        boundaryCheck = GetComponent<BoundaryCheck>();
        rb2d = GetComponent<Rigidbody2D>();

        // Set velocity
        Vector3 vel = Vector3.down;

        vel *= Random.Range(driftMinMax.x, driftMinMax.y);
        rb2d.velocity = vel;

        birthTime = Time.time;
    }

    private void Start()
    {
        float f = Random.value;

        if (GameManager.S.nextLevel > 3)
        {
            if (f < .25f)
            {
                SetType(WeaponType.fastblaster);
            }
            else if (f < .5f)
            {
                SetType(WeaponType.megablaster);
            }
            else if (f < .75f)
            {
                SetType(WeaponType.spread);
            }
            else
            {
                SetType(WeaponType.laser);
            }
        }
        else if (GameManager.S.nextLevel > 2)
        {
            if (f < .1f)
            {
                SetType(WeaponType.fastblaster);
            }
            else if (f < .25f)
            {
                SetType(WeaponType.megablaster);
            }
            else if (f < .4f)
            {
                SetType(WeaponType.spread);
            }
            else
            {
                SetType(WeaponType.laser);
            }
        }
        else if (GameManager.S.nextLevel > 1)
        {
            if (f < .2f)
            {
                SetType(WeaponType.fastblaster);
            }
            else if (f < .4f)
            {
                SetType(WeaponType.megablaster);
            }
            else
            {
                SetType(WeaponType.spread);
            }
        }
        else
        {
            if (f < .5f)
            {
                SetType(WeaponType.fastblaster);
            }
            else
            {
                SetType(WeaponType.megablaster);
            }
        }
    }
    // Update is called once per frame
    void Update () {
        float u = ((Time.time - (birthTime + lifeTime)) / fadeTime);

        if(u>=1)
        {
            Destroy(this.gameObject);
            return;
        }

        if(u >= 0)
        {
            flashTime += Time.deltaTime;
            float z = Mathf.Abs(Mathf.Sin(flashTime * flashSpeed));
            color.a = Mathf.Lerp(0, 1, z);
            spriteRend.color = color;
        }

        if(boundaryCheck.Offscreen())
        {
            Destroy(gameObject);
        }
    }

    public void SetType(WeaponType wt)
    {
        WeaponDefinition def = GameManager.GetWeaponDefinition(wt);
        spriteRend.sprite = def.powerupSprite;
        type = wt;
    }

    public void AbsorbedBy(GameObject target)
    {
        absorbSound.Play(transform);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            Player.S.AbsorbPowerUp(this.gameObject);
        }
    }
}
