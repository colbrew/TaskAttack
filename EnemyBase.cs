using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public static float BOSSSTAGE2MULTIPLIER = .6f;
    public static float BOSSSTAGE3MULTIPLIER = .25f;

    [Header("Set in Inspector")]
    public float speed = 1;
    public float health = 4;
    [Tooltip("Number of seconds to show damage")]
    public float showDamageDuration = 0.05f;
    public int points = 100;
    [Range(0, 1.0f)]
    public float powerUpDropChance = .5f;
    public GameObject standardExplosion;
    public bool usesExplosionAnim = false;
    public GameObject explosionAnim;
    public AudioData explosionSound;
    public AudioData bossInjuryExplosion;
    [Tooltip("Is this enemy with health level that persists over multiple waves?")]
    public bool persistentBossHealth = false;
    public DifficultyLevel difficulty;
    public DifficultyLevel infiniteDifficulty;
    public float enemyFireChance = .02f;
    [Tooltip("Sprites to show when boss enemy damaged")]
    public Sprite[] damageSprites;

    [Header("Set Dynamically")]
    public SpriteRenderer enemyRenderer;
    public bool showingDamage = false;
    [Tooltip("Time to stop showing damage")]
    public float damageDoneTime;
    public bool notifiedOfDestruction = false;
    float levelFireChance = 1;
    BoundaryCheck boundaryCheck;
    public Color originalColor;
    bool takingLaserDamage = false;
    WeaponDefinition laserDef;
    WeaponType type;
    float startHealth;
    public DifficultyLevel currentDifficulty;

    bool explosionStarted = false;
    bool dying = false;

    bool bossStage2 = false;
    bool bossStage3 = false;

    // declare a new delegate type WeaponFireDelegate
    public delegate void WeaponFireDelegate();
    // Create a WeaponFireDelegate field named fireDelegate
    public WeaponFireDelegate fireDelegate;

    // position propert pos
    public Vector3 pos
    {
        get
        {
            return (this.transform.position);
        }

        set
        {
            this.transform.position = value;
        }
    }

    // Use this for initialization
    public virtual void Awake()
    {
        enemyRenderer = gameObject.GetComponent<SpriteRenderer>();
        originalColor = enemyRenderer.color;
        boundaryCheck = GetComponent<BoundaryCheck>();
        if (GameObject.FindWithTag("Level"))
        {
            levelFireChance = GameObject.FindWithTag("Level").GetComponent<LevelManager>().levelEnemyFireChance;
        }
        startHealth = health; // used to calcluate injury stages of a Boss

        if(PlayerPrefs.GetInt("startLevel") == 6)
        {
            currentDifficulty = infiniteDifficulty;
        }
        else
        {
            currentDifficulty = difficulty;
        }
    }

    private void Start()
    {
        if (persistentBossHealth)
        {
            health = gameObject.GetComponentInParent<RandomWaveGenerator>().persistentBossHealth;
            if (health <= startHealth * EnemyBase.BOSSSTAGE3MULTIPLIER && !bossStage3)
            {
                if (damageSprites[1] != null)
                {
                    enemyRenderer.sprite = damageSprites[1];
                }
                enemyRenderer.color = new Color(1, .1f, .1f, 1);
                originalColor = enemyRenderer.color;
                bossStage3 = true;
                bossStage2 = true;
            }
            else if (health <= startHealth * EnemyBase.BOSSSTAGE2MULTIPLIER && !bossStage2)
            {
                if (damageSprites[0] != null)
                {
                    enemyRenderer.sprite = damageSprites[0];
                }
                enemyRenderer.color = new Color(1, .5f, .5f, 1);
                originalColor = enemyRenderer.color;
                bossStage2 = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.tag == "EnemyBoss" && health <= startHealth * EnemyBase.BOSSSTAGE3MULTIPLIER && !bossStage3)
        {
            if (damageSprites[1] != null)
            {
                enemyRenderer.sprite = damageSprites[1];
            }
            enemyRenderer.color = new Color(1, .1f, .1f, 1);
            originalColor = enemyRenderer.color;
            bossInjuryExplosion.Play(transform);
            bossStage3 = true;
            bossStage2 = true;
        }
        else if (gameObject.tag == "EnemyBoss" && health <= startHealth * EnemyBase.BOSSSTAGE2MULTIPLIER && !bossStage2)
        {
            if (damageSprites[0] != null)
            {
                enemyRenderer.sprite = damageSprites[0];
            }
            enemyRenderer.color = new Color(1, .5f, .5f, 1);
            originalColor = enemyRenderer.color;
            bossInjuryExplosion.Play(transform);
            bossStage2 = true;
        }

        if (fireDelegate != null && boundaryCheck.enteredFray)
        {
            if (gameObject.tag == "Enemy")
            {
                if (!Player.Paused) // don't fire when pause button has been pressed
                {
                    if (PlayerPrefs.GetInt("startLevel") == 6) // if this is infinite play, slower progression
                    {
                        if (Random.value < enemyFireChance * currentDifficulty.difficulty * levelFireChance / 2)
                        {
                            fireDelegate();
                        }
                    }
                    else
                    {
                        if (Random.value < enemyFireChance * currentDifficulty.difficulty * levelFireChance)
                        {
                            fireDelegate();
                        }
                    }
                }
            }
        }

        if (showingDamage && Time.time > damageDoneTime && !takingLaserDamage)
        {
            UnShowDamage();
        }

        if(takingLaserDamage)
        {
            health -= (laserDef.damagePerSecond * Time.deltaTime);
            if (health <= 0 && !dying)
            {
                dying = true;
                Die();
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        GameObject otherGO = collision.gameObject;

        if(otherGO.tag == "PlayerProjectileLaser")
        {
            takingLaserDamage = false;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject otherGO = collision.gameObject;


        switch (otherGO.tag)
        {
            case "PlayerProjectile":
                if (!explosionStarted) // check to see if this is enemy boss currently exploding, if not show & take damage
                {
                    // Hurt this enemy
                    ShowDamage();
                    type = otherGO.GetComponent<Projectile>().Type;
                    WeaponDefinition def = GameManager.GetWeaponDefinition(type);
                    health -= def.damageOnHit;
                }

                if (health <= 0 && !dying)
                {
                    dying = true;
                    Die();
                }

                if (persistentBossHealth)
                {
                    gameObject.GetComponentInParent<RandomWaveGenerator>().persistentBossHealth = health;
                }

                if (otherGO.tag == "PlayerProjectile")
                {
                    Destroy(otherGO);
                }

                break;

            case "PlayerProjectileLaser":
                // Hurt this enemy
                ShowDamage();
                takingLaserDamage = true;
                type = otherGO.GetComponent<Projectile>().Type;
                laserDef = GameManager.GetWeaponDefinition(type);

                break;
            
            case "Player":
                if (!Player.S.playerStarting)
                {
                    health -= 10;

                    Player.S.LoseLife();

                    if (persistentBossHealth)
                    {
                        gameObject.GetComponentInParent<RandomWaveGenerator>().persistentBossHealth = health;
                    }

                    if (health <= 0 && !dying)
                    {
                        dying = true;
                        Die();
                    }
                }
                break;

            default:
                break;
        }
    }

    public void DestroyEnemyProjectiles()
    {
        // destroy any enemy projectiles remaining on the screen
        GameObject[] liveProjectiles = GameObject.FindGameObjectsWithTag("EnemyProjectile");
        foreach (GameObject projectile in liveProjectiles)
        {
            Destroy(projectile);
        }
    }

    public void ShowDamage()
    {
        enemyRenderer.color = new Color(1, 1, 1, .3f);
        showingDamage = true;
        damageDoneTime = Time.time + showDamageDuration;
    }

    public void UnShowDamage()
    {
        enemyRenderer.color = originalColor;
        showingDamage = false;
    }

    void Die()
    {
        if (!notifiedOfDestruction)
        {
            GameManager.S.PowerUpDrop(this.gameObject.GetComponent<EnemyBase>());
        }
        notifiedOfDestruction = true;

        if (usesExplosionAnim && !explosionStarted)
        {
            Instantiate(explosionAnim).transform.SetParent(this.transform);
            GameManager.S.AddScore(points);
            explosionStarted = true;
        }
        else if(!explosionStarted)
        {
            if (standardExplosion != null)
            {
                Instantiate(standardExplosion, transform.position, Quaternion.identity);
            }
            GameManager.S.AddScore(points);
            explosionSound.Play(transform);
            this.gameObject.SetActive(false);
            explosionStarted = true;
        }
    }
}

