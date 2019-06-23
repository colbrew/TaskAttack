using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallsScript : EnemyBase {

    Rigidbody2D rb;
    public float maxVelocity;
    float sqrMaxVelocity;

    // Use this for initialization
    void Start () 
    {
        rb = GetComponent<Rigidbody2D>();
        float x = Random.Range(-1f, 1f);
        rb.velocity = new Vector2(x, 1 - Mathf.Pow(x, 2)) * speed;
        SetMaxVelocity(maxVelocity);
    }

    void SetMaxVelocity(float maxV)
    {
        this.maxVelocity = maxV;
        sqrMaxVelocity = maxVelocity * maxVelocity;
    }

    void FixedUpdate()
    {
        var v = rb.velocity;
        // Clamp the velocity, if necessary
        // Use sqrMagnitude instead of magnitude for performance reasons.
        if (v.sqrMagnitude > sqrMaxVelocity)
        { // Equivalent to: rigidbody.velocity.magnitude > maxVelocity, but faster.
          // Vector3.normalized returns this vector with a magnitude 
          // of 1. This ensures that we're not messing with the 
          // direction of the vector, only its magnitude.
            rb.velocity = v.normalized * maxVelocity;
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject otherGO = collision.gameObject;

        switch (otherGO.tag)
        {
            case "PlayerProjectile":

                // Hurt this enemy
                ShowDamage();

                WeaponType type = otherGO.GetComponent<Projectile>().Type;
                WeaponDefinition def = GameManager.GetWeaponDefinition(type);
                health -= def.damageOnHit;

                if (health <= 0)
                {
                    if (standardExplosion != null)
                    {
                        Instantiate(standardExplosion, transform.position, Quaternion.identity);
                    }
                    if (!notifiedOfDestruction)
                    {
                        GameManager.S.PowerUpDrop(this.gameObject.GetComponent<EnemyBase>());
                    }
                    notifiedOfDestruction = true;
                    this.gameObject.SetActive(false);
                    GameManager.S.AddScore(points);
                    explosionSound.Play(transform);
                }

                Destroy(otherGO);

                break;

            case "Player":
                if (!Player.S.playerStarting)
                {
                    health -= 10;

                    Player.S.LoseLife();

                    if (health <= 0)
                    {
                        if (standardExplosion != null)
                        {
                            Instantiate(standardExplosion, transform.position, Quaternion.identity);
                        }
                        this.gameObject.SetActive(false);
                        GameManager.S.AddScore(points);
                        explosionSound.Play(transform);
                    }
                }
                break;

            default:
                break;
        }
    }
}