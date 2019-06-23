using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetInactiveOnNoActiveChildren : MonoBehaviour {

    int childCount;

    // Use this for initialization
    void Start () {
        childCount = transform.childCount;
    }
    
    // Update is called once per frame
    void Update () {
        for (int i = 0; i < childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeInHierarchy)
            {
                break;
            }
            if (i == childCount - 1)
            {
                gameObject.SetActive(false);
            }
        }
            
    }
}
