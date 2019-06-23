using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyButton : MonoBehaviour {

    public void DestroyTheButton()
    {
        gameObject.SetActive(false);
    }
}
