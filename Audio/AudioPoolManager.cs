using System.Collections.Generic;
using UnityEngine;

public class AudioPoolManager
{
    private static AudioPoolManager _instance;
    public static Transform AUDIO_ANCHOR;

    public static AudioPoolManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AudioPoolManager();
            }
            return _instance;
        }
    }

    public List<AudioSourceController> _pool;

    public AudioSourceController GetController()
    {
        if (AUDIO_ANCHOR == null)
        {
            GameObject go = new GameObject("_AudioAnchor");
            AUDIO_ANCHOR = go.transform;
        }

        AudioSourceController output = null;
        if (_pool.Count > 0)
        {
            output = _pool[0];
            _pool.Remove(output);
            return output;
        }
        else
        {
            GameObject go = new GameObject("AudioController");
            output = go.AddComponent<AudioSourceController>();
            go.transform.SetParent(AUDIO_ANCHOR, true);
            return output;
        }
    }

    public void PutController(AudioSourceController controller)
    {
        if (_pool.Contains(controller) == false)
            _pool.Add(controller);
    }
}
