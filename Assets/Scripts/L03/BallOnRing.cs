using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallOnRing : MonoBehaviour
{
    Vector3 center = Vector3.zero;
    float radius = 2.5f;
    Vector3 x;
    Vector3 v;
    Vector3 g = new Vector3(0, -10.0f, 0);
    float dt = 1.0f / 120.0f;
    public GameObject Ball;
    // Start is called before the first frame update
    void Start()
    {
        x = Ball.transform.position;
        v = Vector3.zero;
        Application.targetFrameRate = 120;
    }

    // Update is called once per frame
    void Update()
    {
        v += g * dt;
        Vector3 previousX = x;
        x += v * dt;
        //Solve constraint
        Vector3 dir = x - center;
        dir = dir.normalized;
        x = dir * radius;
        v = (x - previousX) / dt;
        Ball.transform.position = x;
    }
}
