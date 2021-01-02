using System;
using System.Collections.Generic;
using UnityEngine;

public class Timeline : MonoBehaviour
{
    private readonly List<(long, Vector3)> positions = new List<(long, Vector3)>();

    private long start;

    private int index = 1;
    public bool inverted = false;

    public GameObject mario;

    public int Size => inverted ? index : positions.Count;

    void Start()
    {
        start = DateTime.Now.Ticks;
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            Invert();
    }

    void FixedUpdate()
    {
        if (!inverted)
        {
            positions.Add((DateTime.Now.Ticks - start, mario.transform.localPosition));
        }
        else
        {
            if (index >= 0)
            {
                mario.transform.localPosition = positions[index].Item2;
                --index;
            }
            else
            {
                Invert();
            }
        }
        
    }

    private void Invert()
    {
        if (inverted)
        {
            inverted = false;
            mario.GetComponent<Rigidbody2D>().isKinematic = false;
            mario.GetComponent<Mario>().inputFreezed = false;
            index = 1;
            positions.Clear();
        }
        else
        {
            inverted = true;
            mario.GetComponent<Rigidbody2D>().isKinematic = true;
            mario.GetComponent<Mario>().inputFreezed = true;
            index = positions.Count - 1;
        }
    }
}
