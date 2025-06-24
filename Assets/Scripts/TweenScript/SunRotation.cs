using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotation : MonoBehaviour
{
    public GameObject sunObject;
    public float time;
    public float add;
    // Start is called before the first frame update
    void Start()
    {
        LeanTween.rotateAround(sunObject, -Vector3.forward, add, time).setLoopClamp();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
