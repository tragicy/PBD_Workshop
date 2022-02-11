using System.Collections.Generic;
using UnityEngine;

public class BallsCollision : MonoBehaviour
{
    List<Ball> balls;
    public GameObject BallPrefab;
    //float dt = 1.0f / 60.0f;
    Vector3 g = new Vector3(0,-10,0);
    class Ball
    {
        GameObject go;
        public Vector3 v;
        public Vector3 x;
        float dt = 1.0f / 60.0f;
        public float mass = 1.0f;
        public float radius = 0.5f;
        Vector3 g = new Vector3(0, 0, 0);

        public Ball(Vector3 _x,Vector3 _v,GameObject goPrefab) 
        {
            v = _v;
            x = _x;
            go = Instantiate(goPrefab);
            go.transform.position = x;
            go.SetActive(true);
        }
        public void Simulate()
        {
            v += g * dt;
            x += v * dt;
            go.transform.position = x;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        balls = new List<Ball>();
        float rangev = 1.0f;
        for (int i = 0; i < 25; i++)
        {
            Vector3 rv = new Vector3(Random.Range(-rangev, rangev), Random.Range(-rangev, rangev), 0);
            Ball ball = new Ball(new Vector3(Random.Range(-5.0f, 5.0f), Random.Range(-3.0f, 3.0f), 0), rv, BallPrefab);
            balls.Add(ball);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //foreach(var item in balls)
        //{
        //    item.Simulate();
        //    CollisionWalls(item);
        //}
        for (int i = 0; i < balls.Count; i++)
        {
            balls[i].Simulate();
            for (int j = i; j < balls.Count; j++)
            {
                CollisionBall(balls[i],balls[j]);
            }
            CollisionWalls(balls[i]);
        }
    }
    void CollisionBall(Ball ball1, Ball ball2, float elastic = 1f)
    {
        Vector3 distance = ball2.x - ball1.x;
        if (distance.magnitude < ball1.radius + ball2.radius)
        {
            float corr = ball1.radius + ball2.radius - distance.magnitude;
            corr /= 2.0f;
            
            //update x
            Vector3 dir = distance.normalized;
            ball1.x -= dir * corr;
            ball2.x += dir * corr;
            float v1 = Vector3.Dot(ball1.v, dir);
            float v2 = Vector3.Dot(ball2.v, dir);
            float m1 = ball1.mass;
            float m2 = ball2.mass;

            float newV1 = (m1 * v1 + m2 * v2 - m2 * (v1 - v2) * elastic) / (m1 + m2);
            float newV2 = (m1 * v1 + m2 * v2 - m1 * (v2 - v1) * elastic) / (m1 + m2);
            ball1.v = ball1.v + dir * (newV1 - v1);
            ball2.v = ball2.v + dir * (newV2 - v2);
        }
    }
    void CollisionWalls(Ball ball)
    {
        float wallHeight = 4.0f;
        float wallLength = 7.0f;

        if (ball.x.x < -wallLength + ball.radius)
        {
            ball.x.x = -wallLength + ball.radius;
            ball.v.x = -ball.v.x;
        }

        if (ball.x.x > wallLength - ball.radius)
        {
            ball.x.x = wallLength - ball.radius;
            ball.v.x = -ball.v.x;
        }

        if (ball.x.y < -wallHeight + ball.radius)
        {
            ball.x.y = -wallHeight + ball.radius;
            ball.v.y = -ball.v.y;
        }

        if (ball.x.y > wallHeight - ball.radius)
        {
            ball.x.y = wallHeight - ball.radius;
            ball.v.y = -ball.v.y;
        }
    }
}
