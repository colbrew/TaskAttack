using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is an enum of the various possible weapon types
public enum WeaponType
{
    none,
    blaster,    // A simple blaster
    megablaster, // a big blaster
    fastblaster, // a fast blaster
    spread, // a spread shot
    laser,
    enemyblaster,
    enemyspread,
    enemylaser
}

[System.Serializable]
public class WeaponDefinition
{
    public WeaponType type = WeaponType.none;
    public Sprite powerupSprite;
    [Tooltip("Amount of damage caused")]
    public float damageOnHit = 0;
    [Tooltip("For lasers")]
    public float damagePerSecond = 0;
    public float delayBetweenShots = 0;
    [Tooltip("Speed of projectiles")]
    public float velocity = 20;
    public float powerUpLife = 10;
    public AudioData fireSound;
}

public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Set in Inspector")]
    public GameObject[] projectilePrefabs = new GameObject[1];
    [Tooltip("Splash for milk/beer laser")]
    public GameObject splashPrefab;
    public bool rotateOnLaserFire = false;
    public bool animateOnFire = false;
    public GameObject enemyObject;
    [SerializeField]
    private WeaponType _type = WeaponType.none;

    [Header("Set Dynamically")]
    [SerializeField]
    public WeaponDefinition def;
    public float lastShotTime;
    GameObject player;
    GameObject projectile;
    GameObject currentProjectilePrefab;
    bool laserFiring = false;
    public GameObject currentLaserProjectile;
    DifficultyLevel difficulty;
    float currentBottleRotation;

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
        player = GameObject.Find("Player");
        currentProjectilePrefab = projectilePrefabs[0];

    }

    // Use this for initialization
    void Start()
    {
        if (transform.parent.tag != "Player")
        {
            difficulty = GetComponentInParent<EnemyBase>().currentDifficulty;
        }

        SetType(Type);

        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        GameObject rootGO = transform.root.gameObject;
        if (rootGO.GetComponent<Player>() != null)
        {
            rootGO.GetComponent<Player>().fireDelegate += Fire;
        }

        if (gameObject.GetComponentInParent<EnemyBase>() != null)
        {
            gameObject.GetComponentInParent<EnemyBase>().fireDelegate += Fire;
        }
    }

    public void SetType(WeaponType wt)
    {
        _type = wt;
        if (wt == WeaponType.none)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }

        def = GameManager.GetWeaponDefinition(_type);
        lastShotTime = 0;
    }


    public void Update()
    {
        if(player == null)
        {
            player = GameObject.Find("Player");
        }

        // turns player laser off when paused and back on when unpaused
        if (Player.Paused && Type == WeaponType.laser)
        {
            if (currentLaserProjectile)
            {
                currentLaserProjectile.SetActive(false);
            }
        }
        else if (Type == WeaponType.laser && currentLaserProjectile != null)
        {
            currentLaserProjectile.SetActive(true);
        }

        // if win level with laser turned on, turn it off and back to a blaster.
        if(Type == WeaponType.laser && (Player.S.celebration || Player.S.isTextInterlude))
        {
            Type = WeaponType.blaster;
        }
    }

    public void Fire()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (Time.time - lastShotTime < def.delayBetweenShots)
        {
            return;
        }
        Projectile p;
        Vector3 vel = Vector3.up * def.velocity;

        // instantiate appropriate projectiles for given weapon type
        switch (Type)
        {
            case WeaponType.blaster:
            case WeaponType.megablaster:
            case WeaponType.fastblaster:
                if (laserFiring)
                {
                    currentProjectilePrefab = projectilePrefabs[0]; // return player projectile prefab to blaster at end of laser life
                    StopLaser();
                }
                p = MakeProjectile();
                FireSound();
                if (Type == WeaponType.megablaster)
                {
                    p.transform.localScale *= 2.5f;
                }
                p.rb.velocity = vel;

                break;

            case WeaponType.enemyblaster:
                p = MakeProjectile();
                if (player != null)
                {
                    // projectile targets player
                    Vector3 playerTarget = player.transform.position - transform.position;
                    // prevent enemy from shooting up
                    if (playerTarget.y > 0)
                    {
                        Destroy(p.gameObject);
                    }
                    else
                    {
                        FireSound();
                    }
                    playerTarget.Normalize();
                    p.rb.velocity = playerTarget * def.velocity * Mathf.Sqrt(difficulty.difficulty);
                    float rot_z = Mathf.Atan2(playerTarget.y, playerTarget.x) * Mathf.Rad2Deg;
                    p.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
                }
                else
                {
                    Destroy(p.gameObject);
                }
                break;

            case WeaponType.spread:
                if (laserFiring)
                {
                    currentProjectilePrefab = projectilePrefabs[0]; // return player projectile prefab to blaster at end of laser life
                    StopLaser();
                }
                FireSound();
                p = MakeProjectile();
                p.rb.velocity = vel;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(10, Vector3.forward);
                p.rb.velocity = p.transform.rotation * vel;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.forward);
                p.rb.velocity = p.transform.rotation * vel;
                break;

            case WeaponType.enemyspread:
                vel = vel * Mathf.Sqrt(difficulty.difficulty);
                currentProjectilePrefab = projectilePrefabs[0];
                p = MakeProjectile();
                p.rb.velocity = -vel;
                if(projectilePrefabs.Length >= 2)
                {
                    currentProjectilePrefab = projectilePrefabs[1];
                }
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(30, Vector3.forward);
                p.rb.velocity = p.transform.rotation * -vel;
                if (projectilePrefabs.Length >= 3)
                {
                    currentProjectilePrefab = projectilePrefabs[2];
                }
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(-30, Vector3.forward);
                p.rb.velocity = p.transform.rotation * -vel;
                FireSound();
                break;

            case WeaponType.laser:
                currentProjectilePrefab = projectilePrefabs[1];
                if (!laserFiring)
                {
                    laserFiring = true;
                    MakeProjectile();
                }
                break;

            case WeaponType.enemylaser:
                if (!laserFiring)
                {
                    if (rotateOnLaserFire)
                    {
                        currentBottleRotation = -110 - Random.value * 140;
                        enemyObject.transform.Rotate(0, 0, currentBottleRotation);
                    }
                    if(animateOnFire)
                    {
                        enemyObject.GetComponent<Animator>().enabled = true;
                    }
                    laserFiring = true;
                    MakeProjectile();
                }
                break;
        }
    }

    public void StopLaser()
    {
        if (currentLaserProjectile != null)
        {
            Destroy(currentLaserProjectile);
        }
        if (rotateOnLaserFire)
        {
            enemyObject.transform.Rotate(0, 0, -currentBottleRotation);
        }
        if (animateOnFire)
        {
            enemyObject.GetComponent<Animator>().enabled = false;
        }
        laserFiring = false;
    }

    void FireSound()
    {
        if (def.fireSound != null)
        {
            def.fireSound.Play(transform);
        }
    }

    public Projectile MakeProjectile()
    {
        projectile = Instantiate<GameObject>(currentProjectilePrefab);
        projectile.transform.position = this.transform.position;
        projectile.transform.SetParent(PROJECTILE_ANCHOR, true);
        if (transform.parent.gameObject.tag == "Player")
        {
            projectile.tag = "PlayerProjectile";
            if (Type == WeaponType.laser)
            {
                projectile.tag = "PlayerProjectileLaser";
                currentLaserProjectile = projectile;
                currentLaserProjectile.transform.SetParent(this.transform, true);
            }
            projectile.layer = LayerMask.NameToLayer("Player Projectile");
        }
        else
        {
            projectile.tag = "EnemyProjectile";
            if (Type == WeaponType.enemylaser)
            {
                projectile.tag = "EnemyProjectileLaser";
                currentLaserProjectile = projectile;
                currentLaserProjectile.transform.SetParent(this.transform, true);
            }
            projectile.layer = LayerMask.NameToLayer("Enemy Projectile");
        }

        Projectile p = projectile.GetComponent<Projectile>();
        p.Type = Type;
        lastShotTime = Time.time;
        return (p);
    }
}

