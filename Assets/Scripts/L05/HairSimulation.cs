using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HairSimulation : MonoBehaviour
{
    public GameObject Ball;

    Vector3[] X;
    Vector3[] preX;
    Vector3[] V;
    Vector3[] D;
    float[]   Inv_Mass;
    int[]     E;
    float[]   L;
    List<GameObject> Balls;

    //const value
    float length = 1.0f;
    const int edgeCount = 30;
    int pointCount = edgeCount + 1;
    Vector3 g = new Vector3(0, -10.0f,0);
    float dt = 1.0f / 120.0f;
    int stepCount = 1;
    float inv_k = 0.0f;
    float Sdamping = 0.05f;
    // Start is called before the first frame update
    void Start()
    {
        //Application.targetFrameRate = 120;
        dt /= (float)stepCount;
        X = new Vector3[pointCount];
        V = new Vector3[pointCount];
        Inv_Mass = new float[pointCount];
        E = new int[2*edgeCount];
        L = new float[edgeCount];
        D = new Vector3[edgeCount];
        Balls = new List<GameObject>();
        //Inv_Mass[0] = 0;
        //Inv_Mass[1] = 0.5f;
        //Inv_Mass[2] = 1.0f;
        //Inv_Mass[3] = 2.0f;

        for (int i = 0; i < pointCount; i++)
        {
            X[i] = new Vector3(i*1.0f, i*length * 0, 0);
            V[i] = Vector3.zero;
            float s = 0.1f;
            float mass = 1;
            for (int j = 0; j < pointCount; j++)
            {
                mass *= s;
            }
            Inv_Mass[i] = 1.0f/mass;
            GameObject ball = Instantiate(Ball, X[i], Quaternion.identity);
            ball.transform.SetParent(transform);
            //if(i!=0)
            //ball.transform.localScale /= (Mathf.Pow(Inv_Mass[i],1.0f));
            ball.transform.localScale /= 2.0f;
            ball.SetActive(true);
            Balls.Add(ball);
            if (i == pointCount - 1)
            {
                ball.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        Inv_Mass[0] = 0;
        for (int i = 0; i < edgeCount; i++)
        {
            E[2 * i] = i;
            E[2 * i + 1] = i + 1;
            L[i] = length;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < pointCount; i++)
        {
            //V[i] = (X[i] - Balls[i].transform.position) / dt;
            X[i] = Balls[i].transform.position;
        }

        //preX = X;
        for (int step = 0; step < stepCount; step++)
        {
            for (int i = 0; i < pointCount; i++)
            {
                if (i == 0)
                    continue;
                V[i] += g * dt;

                X[i] += V[i] * dt;
            }
            //Solve constraint
            for (int i = 0; i < 2; i++)
                SolveConstraint();
            //Update velocity and position
            for (int i = 0; i < pointCount; i++)
            {
                V[i] = (X[i] - Balls[i].transform.position) / dt;
                if (i != pointCount - 1)
                    V[i] += Sdamping * D[i] / dt;
                Balls[i].transform.position = X[i];
            }
        }
        for (int i = 0; i < pointCount; i++)
        {
            V[i] *= 0.99f;
        }
        for (int i = 0; i < edgeCount; i++)
        {
            Debug.DrawLine(X[i],X[i+1]);
        }

    }

    void SolveConstraint()
    {
        for (int i = 0; i < edgeCount; i++)
        {
            int ii = E[2 * i];
            int jj = E[2 * i + 1];
            float Wi = Inv_Mass[ii];
            //if(i<10)
            //Wi = 0.5f;

            float Wj = Inv_Mass[jj];
            Vector3 vec = X[jj] - X[ii];
            float l0 = vec.magnitude;
            Vector3 deltaXi = -(Wi / (Wi + Wj + inv_k / dt)) * (L[i] - l0) * (vec / l0);
            Vector3 deltaXj = +(Wj / (Wi + Wj + inv_k / dt)) * (L[i] - l0) * (vec / l0);
            D[ii] = -deltaXj;
            X[jj] += deltaXj;
            if (ii != 0)
                X[ii] += deltaXi;

        }
    }
}
