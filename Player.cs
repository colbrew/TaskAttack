using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// collider on Player must not be set to Trigger in order for particle collisions to work
public class Player : MonoBehaviour {

    public static Player S;
    public static Vector3 PLAYERSTARTLOCATION = new Vector3(0, -3, 0);

    [Header("Set in Inspector")]
    [Tooltip("Insert the player ship engines prefab here.")]
    public GameObject engines;
    [Tooltip("Number of extra lives the player has.")]
    [SerializeField] private int extraLives;
    [Tooltip("Explosion sound when player dies.")]
    [SerializeField] private AudioData explosionSound;
    [Tooltip("Explosion when player dies.")]
    [SerializeField] private GameObject explosionAnim;
    [SerializeField] private float movingSpeed;
    [SerializeField] private float playerFingerOffset;
    [Tooltip("Flash speed when player spawns.")]
    [SerializeField] private float flashSpeed = 3;
    [Tooltip("Flash duration when player spawns.")]
    [SerializeField] private float flashDuration = 2;

    [Header("Set Dynamically")]
    public bool celebration = false;
    public bool playerStarting;
    public bool instructionsShowing = false;
    [SerializeField] private Weapon weapon;
    [SerializeField] private WeaponDefinition currentWeaponDef;
    
    public bool powerupOn = false;
    private float powerupStart;
    private float powerupLife;
    private BoundaryCheck shipBnd;
    private SpriteRenderer spriteRend;
    private bool playerDead;
    private SpriteRenderer[] engRends = new SpriteRenderer[2];
    private Vector3 startPosition, fingerPosition;
    private float currentFingerOffset;
    private float startTime;
    private Color engineColor;   
    private float flashTime;

    public delegate void WeaponFireDelegate();
    public WeaponFireDelegate fireDelegate;

    public static bool Paused { get; set; } = false;
    public static bool AutoFire { get; set; } = true;
    public bool IsTextInterlude { get; set; } = false;
    public int ExtraLives { get { return extraLives; } set { extraLives = value; }}

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
        engineColor = spriteRend.color;

        ExtraLives = 2;
    }

    private void Start()
    {
        weapon.Type = WeaponType.blaster;
        if (!instructionsShowing)
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
            engineColor.a = Mathf.Lerp(0, 1, u);
            spriteRend.color = engRends[0].color = engRends[1].color = engineColor;

            // if flash time is up and alpha is near 1, back to normal
            if (flashTime > flashDuration && engineColor.a > .95f)
            {
                engineColor.a = 1;
                spriteRend.color = engRends[0].color = engRends[1].color = engineColor;
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
            if(playerDead || celebration || instructionsShowing)
            {
                return;	
            }
            else if (IsTextInterlude) // if this is a pause due to a text interlude
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
            GameManager.S.PowerUpCounter.text = Mathf.CeilToInt((powerupLife - (Time.time - powerupStart))).ToString();

            // check for end of powerup lifetime or text interlude, then reset to blaster
            if (Time.time - powerupStart > powerupLife || IsTextInterlude)
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
        currentWeaponDef = GameManager.GetWeaponDefinition(weapon.Type);
        powerupLife = currentWeaponDef.powerUpLife;
        powerupStart = Time.time;
        if (!powerupOn)
        {
            GameManager.powerUpIcon.enabled = !GameManager.powerUpIcon.enabled;
        }
        GameManager.powerUpIcon.sprite = currentWeaponDef.powerupSprite;
        powerupOn = true;
        pu.AbsorbedBy(this.gameObject);
    }

    public void TurnOffPowerUp()
    {
        weapon.Type = WeaponType.blaster;
        powerupOn = false;
        GameManager.S.PowerUpCounter.text = null;
        GameManager.powerUpIcon.enabled = !GameManager.powerUpIcon.enabled;
    }
    
    public void LoseLife()
    {
        playerDead = true;
        Paused = true;
        BlurScreen.S.UnBlur();
        GameManager.S.LevelText.text = null;

        // explosion
        explosionSound.Play(transform);
        Instantiate(explosionAnim, transform.position, Quaternion.identity);
        Destroy(GameObject.FindWithTag("PlayerProjectileLaser"));

        gameObject.SetActive(false);

        if (ExtraLives <= 0)
        {
            GameManager.S.DelayedRestart(GameManager.S.GameRestartDelay);
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
        engineColor.a = 0;
        spriteRend.color = engRends[0].color = engRends[1].color = engineColor;
        gameObject.SetActive(true);
        playerDead = false;
        if (!IsTextInterlude && !celebration)
        {
            transform.position = PLAYERSTARTLOCATION;
            Paused = false;
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
