using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunScale : MonoBehaviour
{
    public GameObject sunObject;
    public float time;
    public float scale;
    // Start is called before the first frame update
    void Start()
    {
        LeanTween.scale(sunObject, new Vector3(scale, scale, scale), time).setLoopPingPong();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
