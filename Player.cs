using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// collider on Player must not be set to Trigger in order for particle collision to work
public class Player : MonoBehaviour {

    public static Player S;
    public static Vector3 PLAYERSTARTLOCATION = new Vector3(0, -3, 0);

    [Header("Set in Inspector")]
    public Sprite regularSprite;
    public int extraLives;
    public AudioData explosionSound;
    public GameObject explosionAnim; // player death animation
    public float movingSpeed;
    public float playerFingerOffset;
    public GameObject engines;

    [Header("Set Dynamically")]
    Vector3 startPosition, fingerPosition;
    public float currentFingerOffset;
    public float startTime;
    public Weapon weapon;
    BoundaryCheck shipBnd;
    private static bool paused = false;
    public bool celebration = false;
    SpriteRenderer spriteRend;
    private static bool autoFire = true;
    bool playerDead;
    public bool instructions = false;
    SpriteRenderer[] engRends = new SpriteRenderer[2];

    // powerup controls
    public bool powerupOn = false;
    public float powerupStart;
    public float powerupLife;
    public WeaponDefinition def;

    // pause variables
    public bool isTextInterlude = false;

    // player spawn variables
    Color color;
    float flashSpeed = 3;
    float flashDuration = 2;
    public bool playerStarting;
    float flashTime;

    // declare a new delegate type WeaponFireDelegate
    public delegate void WeaponFireDelegate();
    // Create a WeaponFireDelegate field named fireDelegate
    public WeaponFireDelegate fireDelegate;

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

    public static bool AutoFire
    {
        get
        {
            return autoFire;
        }

        set
        {
            autoFire = value;
        }
    }

    public int ExtraLives
    {
        get
        {
            return extraLives;
        }

        set
        {
            extraLives = value;
        }
    }

    private void Awake()
    {
        if(S != null)
        {
            Destroy(gameObject);
        }
        else
        {
            S = this;
        }

        playerDead = true;
        AutoFire = true;

        // set up ship bounds
        shipBnd = GetComponent<BoundaryCheck>();
        shipBnd.keepOnScreen = true;

        weapon = GetComponentInChildren<Weapon>();

        spriteRend = GetComponent<SpriteRenderer>();
        engRends = engines.GetComponentsInChildren<SpriteRenderer>();
        color = spriteRend.color;

        ExtraLives = 2;
    }

    private void Start()
    {
        weapon.Type = WeaponType.blaster;
        if (!instructions)
        {
            Respawn();
        }
        else
        {
            playerDead = false;
        }
    }

    // Update is called once per frame
    void Update () {

        // player flashes and is invincible for set amount of time when starting
        if (playerStarting)
        {
            // make player flash
            flashTime += Time.deltaTime;
            float u = Mathf.Abs(Mathf.Sin(flashTime * flashSpeed));
            color.a = Mathf.Lerp(0, 1, u);
            spriteRend.color = engRends[0].color = engRends[1].color = color;

            // if flash time is up and alpha is near 1, back to normal
            if (flashTime > flashDuration && color.a > .95f)
            {
                color.a = 1;
                spriteRend.color = engRends[0].color = engRends[1].color = color;
                playerStarting = false;
                flashTime = 0;
            }
        }

        if (AutoFire)
        {
                fireDelegate();
        }

        if(Paused)
        {
            if(playerDead || celebration || instructions)
            {
                return;	
            }
            else if (isTextInterlude) // if this is a pause due to a text interlude
            {
                if (Input.touchCount > 0)
                {
                    startPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 10));
                }
            }
        }

        if (!Paused)
        {
            // touch controls

            // first touch when starting level / after a death
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                startPosition = transform.position;
                startTime = Time.time;
                AutoFire = true;
            }

            if (Input.touchCount == 1 && (Input.GetTouch(0).phase == TouchPhase.Stationary || Input.GetTouch(0).phase == TouchPhase.Moved))
            {
                AutoFire = true;

                if(Input.GetTouch(0).position.y <= 125)// as finger reaches bottom of screen, reduce finger offset to 0
                {
                    float z = ((100 - Mathf.Max(Input.GetTouch(0).position.y - 25, 0)) / 100) * playerFingerOffset;
                    currentFingerOffset = playerFingerOffset - z;
                }
                else
                {
                    currentFingerOffset = playerFingerOffset;
                }
                fingerPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y + currentFingerOffset, 10));

                float u = (Time.time - startTime) * movingSpeed;
                if(u > 1)
                {
                    u = 1;
                }
                u = u * u;
                transform.position = Vector3.Lerp(startPosition, fingerPosition, u); 

            }

            // keyboard control testing
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AutoFire = true;
            }

            if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0 || Mathf.Abs(Input.GetAxis("Vertical")) > 0 )
            {
                BlurScreen.S.UnBlur();
                transform.position += new Vector3(Input.GetAxis("Horizontal") * (2 * movingSpeed) * Time.deltaTime, Input.GetAxis("Vertical") * (2 * movingSpeed) * Time.deltaTime, 0);
            }
        }

        if (powerupOn)
        {
            // update powerup countdown text
            GameManager.powerUpCounter.text = Mathf.CeilToInt((powerupLife - (Time.time - powerupStart))).ToString();

            // check for end of powerup lifetime or text interlude, then reset to blaster
            if (Time.time - powerupStart > powerupLife || isTextInterlude)
            {
                TurnOffPowerUp();
            }
        }
    }

    // for smoke projectile coming from muffler boss
    private void OnParticleCollision(GameObject other)
    {
        Debug.Log("Particle Collision");
        if (!playerStarting)
        {
            LoseLife();
        }
    }

    public void AbsorbPowerUp(GameObject go)
    {
        PowerUp pu = go.GetComponent<PowerUp>();
        weapon.Type = pu.type;
        def = GameManager.GetWeaponDefinition(weapon.Type);
        powerupLife = def.powerUpLife;
        powerupStart = Time.time;
        if (!powerupOn)
        {
            GameManager.powerUpIcon.enabled = !GameManager.powerUpIcon.enabled;
        }
        GameManager.powerUpIcon.sprite = def.powerupSprite;
        powerupOn = true;
        pu.AbsorbedBy(this.gameObject);
    }

    public void TurnOffPowerUp()
    {
        weapon.Type = WeaponType.blaster;
        powerupOn = false;
        GameManager.powerUpCounter.text = null;
        GameManager.powerUpIcon.enabled = !GameManager.powerUpIcon.enabled;
    }
    
    public void LoseLife()
    {
        playerDead = true;
        paused = true;
        BlurScreen.S.UnBlur();
        GameManager.levelText.text = null;

        // explosion
        explosionSound.Play(transform);
        Instantiate(explosionAnim, transform.position, Quaternion.identity);
        Destroy(GameObject.FindWithTag("PlayerProjectileLaser"));

        gameObject.SetActive(false);

        if (ExtraLives <= 0)
        {
            GameManager.S.DelayedRestart(GameManager.S.gameRestartDelay);
            Destroy(gameObject);
        }
        else if(ExtraLives > 0)
        {
            // remove one life icon
            GameObject.Find("Life" + ExtraLives.ToString()).SetActive(false);

            // reset weapon to basic blaster (in case they have a power up)
            if(powerupOn)
            {
                TurnOffPowerUp();
            }

            ExtraLives--;
            Invoke("Respawn", 2);
        }   
    }

    public void Respawn()
    {
        color.a = 0;
        spriteRend.color = engRends[0].color = engRends[1].color = color;
        gameObject.SetActive(true);
        playerDead = false;
        if (!isTextInterlude && !celebration)
        {
            transform.position = PLAYERSTARTLOCATION;
            paused = false;
        }
        playerStarting = true;
    }

    public void Celebrate()
    {
        celebration = true;
        AutoFire = false;
        Paused = true;
        BlurScreen.S.UnBlur(); // make sure screen is unblurred
        GetComponent<Celebration>().celebrationStarted = true;
    }
}
