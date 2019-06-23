using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextMessageBox : MonoBehaviour {

    [Header("Set in Inspector")]
    public float colorChangeIncrement = 50;
    public int points = 100;
    public GameObject shatteredTextBox;
    public AudioData textBoxExplosion;

    [Header("Set Dynamically")]
    public SpriteRenderer enemyRenderer;
    public float colorLevel = 255;

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
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject otherGO = collision.gameObject;
        switch (otherGO.tag)
        {
            case "PlayerProjectile":
                colorLevel -= colorChangeIncrement;
                enemyRenderer.color = new Color(1, colorLevel/255f, colorLevel/255f);

                if (colorLevel <= 5)
                {
                    this.gameObject.SetActive(false);
                    Instantiate(shatteredTextBox);
                    textBoxExplosion.Play(transform);
                    enemyRenderer.color = Color.white;
                    colorLevel = 255;
                    GameManager.S.AddScore(points);
                }

                Destroy(otherGO);

                break;

            default:
                break;
        }
    }
}
