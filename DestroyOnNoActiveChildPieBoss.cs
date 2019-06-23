using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnNoActiveChildPieBoss : MonoBehaviour {

    int childCount;
    int activeCount;
    EnemyBase enemyBase;
    float startHealth;

    // Use this for initialization
    void Start () {
        childCount = transform.childCount;
        enemyBase = GetComponent<EnemyBase>();
        startHealth = enemyBase.health;
    }
    
    // Update is called once per frame
    void Update () {
        activeCount = 0;
            
        for (int i = 0; i < childCount; i++)
        {
            if(transform.GetChild(i).gameObject.activeInHierarchy)
            {
                activeCount++;
            }
        }

        if (activeCount == 0)
        {
            gameObject.SetActive(false);
        }
        else if(activeCount <= 2)
        {
            enemyBase.health = .1f * startHealth;
        }
        else if (activeCount < 4)
        {
            enemyBase.health = .4f * startHealth;
        }
    }
}
