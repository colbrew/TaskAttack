using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiExplosion : MonoBehaviour {

    public GameObject[] explosions;
    public GameObject explodingEnemy;

    // Use this for initialization
    void Start () {
        explodingEnemy = transform.parent.gameObject;
        this.transform.position = explodingEnemy.transform.position;
        StartCoroutine("Explode");
    }

    IEnumerator Explode()
    {
        for (int i = 0; i < explosions.Length;i++)
        {
            explosions[i].SetActive(true);
            explodingEnemy.GetComponent<EnemyBase>().explosionSound.Play(transform);
            yield return new WaitForSeconds(.4f);
        }
        explodingEnemy.SetActive(false);
        Destroy(this.gameObject);
        yield break;
    }
}
