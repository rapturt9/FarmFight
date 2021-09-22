using System;
using MLAPI;
using UnityEngine;

public class NetworkTransformTest : NetworkBehaviour
{
    void Update()
    {
        if (IsClient)
        {
            float theta = Time.frameCount / 10.0f;
            transform.position = new Vector3((float)(Math.Cos(theta)), (float)(Math.Sin(theta)), 0f);
        }
    }
}