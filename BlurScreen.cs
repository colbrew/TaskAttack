using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurScreen : MonoBehaviour {

    public static BlurScreen S;

	private void Awake()
	{
        if (S != null)
        {
            Destroy(gameObject);
        }
        else
        {
            S = this;
        }
        UnBlur();
	}

	public void Blur()
    {
        gameObject.SetActive(true);
    }

    public void UnBlur()
    {
        if (gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
    }
}
