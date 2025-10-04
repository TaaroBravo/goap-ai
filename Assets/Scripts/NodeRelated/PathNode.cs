using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class PathNode : MonoBehaviour
{
    public float distanceTraveled = Mathf.Infinity;
    public PathNode previous = null;
    public bool isBlocked;
    public List<Tuple<PathNode,float>> neighbors;
    public LayerMask nodeLayer;
    public int priority;
    public int nodeWeight;

    private void Start()
    {
        if (isBlocked)
            GetComponent<Renderer>().material.color = Color.red;
    }

    public void Reset()
    {
        previous = null;
        distanceTraveled = Mathf.Infinity;
    }    

    public void ResetWeight()
    {
        nodeWeight = 0;
    }
}
