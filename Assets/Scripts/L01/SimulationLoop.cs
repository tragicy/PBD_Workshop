using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationLoop : MonoBehaviour
{
    public Transform ball;
    Vector3 x;
    Vector3 v;
    Vector3 g = new Vector3(0,-10.0f,0);
    float dt = 1.0f / 60.0f;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        v = Vector3.zero;
        x = ball.position;
    }

    // Update is called once per frame
    void Update()
    {
        v = v + g * dt;
        x = x + v * dt;
        if (x.y <= 0)
        {
            v = -0.9f * v;
            x.y = 0;
        }
        ball.position = x;
    }
}
