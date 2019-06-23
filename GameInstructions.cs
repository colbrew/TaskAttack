using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInstructions : MonoBehaviour {

    public static Vector3 PLAYERINSTRUCTIONSLOCATION = new Vector3(0, .8f, 0);
    CanvasGroup canvasGroup;
    bool fadeCanvas = false;
    float startTime;
    float fadeSpeed = 2;

    private void Awake()
    {
        GameManager.S.pauseButton.SetActive(false);

    }

    // Use this for initialization
    void Start () 
    {
        Player.Paused = true;
        Player.S.instructions = true;
        Player.S.transform.position = PLAYERINSTRUCTIONSLOCATION;
        Invoke("FadeCanvas", 4);
        canvasGroup = GetComponent<CanvasGroup>();
	}

    private void Update()
    {
        if(fadeCanvas)
        {
            float u = (Time.time - startTime) / fadeSpeed;
            u = Mathf.Sin(u * Mathf.PI / 2f);//ease out
            if(u >= .99f)
            {
                u = 1;
                Player.Paused = false;
                Player.S.instructions = false;
                GameManager.S.TextInterlude();
                GameManager.S.pauseButton.SetActive(true);
                Destroy(gameObject);
            }
            canvasGroup.alpha = Mathf.Lerp(1, 0, u);
            Player.S.transform.position = Vector3.Lerp(PLAYERINSTRUCTIONSLOCATION, Player.PLAYERSTARTLOCATION, u);
        }
    }

    public void FadeCanvas()
    {
        fadeCanvas = true;
        startTime = Time.time;
    }
}
