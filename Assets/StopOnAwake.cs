using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopOnAwake : MonoBehaviour
{
    // Start is called before the first frame update
    public ParticleSystem Particles;
    void Start()
    {
        Particles.Stop();
    }
}
