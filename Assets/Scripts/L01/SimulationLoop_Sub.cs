using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationLoop_Sub : MonoBehaviour
{
    public Transform ball;
    Vector3 x;
    Vector3 v;
    Vector3 g = new Vector3(0, -10.0f, 0);
    int n = 5;
    float dt = 1.0f / 60.0f;
    float sdt;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        v = Vector3.zero;
        x = ball.position;
        sdt = dt / n;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < n; i++)
        {
            v = v + g * sdt;
            x = x + v * sdt;
            if (x.y <= 0)
            {
                v = -0.9f * v;
                x.y = 0;
            }
        }
        ball.position = x;
    }
}
