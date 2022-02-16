using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FVM : MonoBehaviour
{
	float dt 			= 1.0f/60.0f;
    float mass 			= 1;
	float stiffness_0	= 20000.0f;
    float stiffness_1 	= 5000.0f;
    float damp			= 0.999f;
    Vector3 g = new Vector3(0, -10.0f, 0);

    int[] 		Tet;
	int tet_number;         //The number of tetrahedra
    int[] E;
    float[] L;
    float[] Volume;
    Vector3[] 	Force;
	Vector3[] 	V;
	Vector3[] 	X;
	int number;				//The number of vertices
    float[] W;
	Matrix4x4[] inv_Dm;

	//For Laplacian smoothing.
	Vector3[]   V_sum;
	int[]		V_num;

	SVD svd = new SVD();
    Dictionary<Vector2, bool> edgeDic = new Dictionary<Vector2, bool>();

    public Transform ControlSphere;
    // Start is called before the first frame update
    void Start()
    {
        //Application.targetFrameRate = 60;
    	// FILO IO: Read the house model from files.
    	// The model is from Jonathan Schewchuk's Stellar lib.
    	{
    		string fileContent = File.ReadAllText("Assets/Meshes/house2.ele");
    		string[] Strings = fileContent.Split(new char[]{' ', '\t', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
    		
    		tet_number=int.Parse(Strings[0]);
        	Tet = new int[tet_number*4];

    		for(int tet=0; tet<tet_number; tet++)
    		{
				Tet[tet*4+0]=int.Parse(Strings[tet*5+4])-1;
				Tet[tet*4+1]=int.Parse(Strings[tet*5+5])-1;
				Tet[tet*4+2]=int.Parse(Strings[tet*5+6])-1;
				Tet[tet*4+3]=int.Parse(Strings[tet*5+7])-1;
			}
    	}
    	{
			string fileContent = File.ReadAllText("Assets/Meshes/house2.node");
    		string[] Strings = fileContent.Split(new char[]{' ', '\t', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
    		number = int.Parse(Strings[0]);
    		X = new Vector3[number];
       		for(int i=0; i<number; i++)
       		{
       			X[i].x=float.Parse(Strings[i*5+5])*0.4f;
       			X[i].y=float.Parse(Strings[i*5+6])*0.4f;
       			X[i].z=float.Parse(Strings[i*5+7])*0.4f;
       		}
    		//Centralize the model.
	    	Vector3 center=Vector3.zero;
	    	for(int i=0; i<number; i++)		center+=X[i];
	    	center=center/number;
	    	for(int i=0; i<number; i++)
	    	{
	    		X[i]-=center;
	    		float temp=X[i].y;
	    		X[i].y=X[i].z;
	    		X[i].z=temp;
	    	}
		}
        //tet_number = 1;
        //Tet = new int[tet_number * 4];
        //Tet[0] = 0;
        //Tet[1] = 1;
        //Tet[2] = 2;
        //Tet[3] = 3;

        //number = 4;
        //X = new Vector3[number];
        //V = new Vector3[number];
        //Force = new Vector3[number];
        //X[0] = new Vector3(0, 0, 0);
        //X[1] = new Vector3(1, 0, 0);
        //X[2] = new Vector3(0, 1, 0);
        //X[3] = new Vector3(0, 0, 1);


        //Create triangle mesh.
        Vector3[] vertices = new Vector3[tet_number*12];
        int vertex_number=0;
        for(int tet=0; tet<tet_number; tet++)
        {
        	vertices[vertex_number++]=X[Tet[tet*4+0]];
        	vertices[vertex_number++]=X[Tet[tet*4+2]];
        	vertices[vertex_number++]=X[Tet[tet*4+1]];

        	vertices[vertex_number++]=X[Tet[tet*4+0]];
        	vertices[vertex_number++]=X[Tet[tet*4+3]];
        	vertices[vertex_number++]=X[Tet[tet*4+2]];

        	vertices[vertex_number++]=X[Tet[tet*4+0]];
        	vertices[vertex_number++]=X[Tet[tet*4+1]];
        	vertices[vertex_number++]=X[Tet[tet*4+3]];

        	vertices[vertex_number++]=X[Tet[tet*4+1]];
        	vertices[vertex_number++]=X[Tet[tet*4+2]];
        	vertices[vertex_number++]=X[Tet[tet*4+3]];
        }

        int[] triangles = new int[tet_number*12];
        for(int t=0; t<tet_number*4; t++)
        {
        	triangles[t*3+0]=t*3+0;
        	triangles[t*3+1]=t*3+1;
        	triangles[t*3+2]=t*3+2;
        }
        Mesh mesh = GetComponent<MeshFilter> ().mesh;
		mesh.vertices  = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals ();


		V 	  = new Vector3[number];
        Force = new Vector3[number];
        V_sum = new Vector3[number];
        V_num = new int[number];
        Volume = new float[tet_number];
        //Init volume
        for (int i = 0; i < tet_number;i++)
        {
            Volume[i] = sixVolume(i);
        }
        //Debug.Log(Volume.Length);
        for (int i = 0; i < tet_number; i++)
        {
            int a = Tet[i * 4 + 0];
            int b = Tet[i * 4 + 1];
            int c = Tet[i * 4 + 2];
            int d = Tet[i * 4 + 3];
            SortAndAdd(a, b);
            SortAndAdd(a, c);
            SortAndAdd(a, d);
            SortAndAdd(b, c);
            SortAndAdd(b, d);
            SortAndAdd(c, b);
        }

        //Build edges
        List<Vector2> edgeList = new List<Vector2>();
        foreach (var item in edgeDic.Keys)
        {
            edgeList.Add(item);
        }
        edgeList.Sort((a, b) => {
            if (a.x == b.x)
            {
                if (a.y < b.y)
                    return -1;
                else if (a.y == b.y)
                    return 0;
                else
                    return 1;
            }
            else if (a.x < b.x)
                return -1;
            else
                return 1;
        });
        Debug.Log(edgeList);
        E = new int[edgeList.Count *2];
        L = new float[edgeList.Count];
        for (int i = 0; i < edgeList.Count; i++)
        {
            E[2 * i] = (int)edgeList[i].x;
            E[2 * i + 1] = (int)edgeList[i].y;
            L[i] = (X[E[2 * i]] - X[E[2 * i + 1]]).magnitude;
        }
        Debug.Log(E.Length);
        Debug.Log(L.Length);
        //TODO: Need to allocate and assign inv_Dm
        W = new float[X.Length];
        for (int i = 0; i < X.Length; i++)
        {
            W[i] = 1;
        }
    }
    float sixVolume(int tetIndex)
    {
        int x1 = Tet[tetIndex * 4 + 0];
        int x2 = Tet[tetIndex * 4 + 1];
        int x3 = Tet[tetIndex * 4 + 2];
        int x4 = Tet[tetIndex * 4 + 3];
        float result = Vector3.Dot(Vector3.Cross((X[x2] - X[x1]), (X[x3] - X[x1])), (X[x4] - X[x1]));
        return result;
    }
    void SortAndAdd(float a, float b)
    {
        if (a > b)
        {
            float c = b;
            b = a;
            a = c;
        }
        if (!edgeDic.ContainsKey(new Vector2(a, b)))
            edgeDic.Add(new Vector2(a, b), true);
    }
    Matrix4x4 Build_Edge_Matrix(int tet)
    {
    	Matrix4x4 ret=Matrix4x4.zero;
    	//TODO: Need to build edge matrix here.

		return ret;
    }


    void _Update()
    {
    	// Jump up.
		if(Input.GetKeyDown(KeyCode.Space))
    	{
    		for(int i=0; i<number; i++)
    			V[i].y+=10.0f;
    	}
        if (Input.GetMouseButton(1))
        {
            W[0] = 0;
            X[0] = ControlSphere.position;
        }
        else
            W[0] = 1;
        for (int i=0 ;i<number; i++)
    	{
            //TODO: Add gravity to Force.

        }

        for (int tet=0; tet<tet_number; tet++)
    	{
    		//TODO: Deformation Gradient
    		
    		//TODO: Green Strain

    		//TODO: Second PK Stress

    		//TODO: Elastic Force
			
    	}
        Vector3[] preX = new Vector3[X.Length];
    	for(int i=0; i<number; i++)
    	{
            preX[i] = X[i];
            //TODO: Update X and V here.
            V[i] += g * dt;
            X[i] += V[i] * dt;

            //TODO: (Particle) collision with floor.
            if (X[i].y <= -5)
            {
                X[i].y = -5;
                V[i].y = -0.75f * V[i].y;
            }
        }

        //Solve Contraint
        for(int i =0;i<16;i++)
            SolveDistanceConstraint();
        for(int i=0;i<1;i++)
            SolveVolumeConstraint();
        for (int i = 0; i < V.Length; i++)
        {
            V[i] = (X[i] - preX[i]) / dt;
            V[i] *= 0.99f;
        }
        
    }
    void SolveDistanceConstraint()
    {
        for (int i = 0; i < L.Length; i++)
        {
            int ii = E[2 * i];
            int jj = E[2 * i + 1];
            float Wi = W[ii];
            float Wj = W[jj];
            Vector3 vec = X[jj] - X[ii];
            float l0 = vec.magnitude;
            Vector3 deltaXi = -(Wi / (Wi + Wj + 0.5f / dt)) * (L[i] - l0) * (vec / l0);
            Vector3 deltaXj = +(Wj / (Wi + Wj + 0.5f / dt)) * (L[i] - l0) * (vec / l0);
            X[jj] += deltaXj;
            X[ii] += deltaXi;
        }
    }
    void SolveVolumeConstraint()
    {
        for (int i = 0; i < tet_number; i++)
        {
            int x1 = Tet[i * 4 + 0];
            int x2 = Tet[i * 4 + 1];
            int x3 = Tet[i * 4 + 2];
            int x4 = Tet[i * 4 + 3];
            Vector3 G1C = Vector3.Cross((X[x4] - X[x2]), (X[x3] - X[x2]));
            Vector3 G2C = Vector3.Cross((X[x3] - X[x1]), (X[x4] - X[x1]));
            Vector3 G3C = Vector3.Cross((X[x4] - X[x1]), (X[x2] - X[x1]));
            Vector3 G4C = Vector3.Cross((X[x2] - X[x1]), (X[x3] - X[x1]));

            float V0 = sixVolume(i);
            float lambda = (V0 - Volume[i])/((SquareLength(G1C)*W[x1]+SquareLength(G2C) *W[x2]+SquareLength(G3C) *W[x3]+SquareLength(G4C)*W[x4]));
            X[x1] -= W[x1] * G1C * lambda;
            X[x2] -= W[x2] * G2C * lambda;
            X[x3] -= W[x3] * G3C * lambda;
            X[x4] -= W[x4] * G4C * lambda;
        }
    }
    float SquareLength(Vector3 v)
    {
        return v.x * v.x + v.y * v.y + v.z * v.z;
    }
    // Update is called once per frame
    void Update()
    {
    	for(int l=0; l<1; l++)
    		 _Update();

    	// Dump the vertex array for rendering.
    	Vector3[] vertices = new Vector3[tet_number*12];
        int vertex_number=0;
        for(int tet=0; tet<tet_number; tet++)
        {
        	vertices[vertex_number++]=X[Tet[tet*4+0]];
        	vertices[vertex_number++]=X[Tet[tet*4+2]];
        	vertices[vertex_number++]=X[Tet[tet*4+1]];
        	vertices[vertex_number++]=X[Tet[tet*4+0]];
        	vertices[vertex_number++]=X[Tet[tet*4+3]];
        	vertices[vertex_number++]=X[Tet[tet*4+2]];
        	vertices[vertex_number++]=X[Tet[tet*4+0]];
        	vertices[vertex_number++]=X[Tet[tet*4+1]];
        	vertices[vertex_number++]=X[Tet[tet*4+3]];
        	vertices[vertex_number++]=X[Tet[tet*4+1]];
        	vertices[vertex_number++]=X[Tet[tet*4+2]];
        	vertices[vertex_number++]=X[Tet[tet*4+3]];
        }
        Mesh mesh = GetComponent<MeshFilter> ().mesh;
		mesh.vertices  = vertices;
		mesh.RecalculateNormals ();
    }
}
