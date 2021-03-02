using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeMusic : MonoBehaviour
{
    private AudioSource music;
    private float temp = 0.45f;

    private void Start()
    {
        music = GetComponent<AudioSource>();    
    }

    private void Update()
    {
        music.volume = temp;
    }

    public void SetValume(float aim)
    {
        temp = aim;
    }
}
