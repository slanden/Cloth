using UnityEngine;
using System.Collections.Generic;

public class ClothSim : MonoBehaviour
{
    public int divisions = 0;
    public int cols = 2;
    public int rows = 2;
    int numVerts = 4;
    public SParticle[] particles;

    public GameObject[] pGizmos;
    public GameObject ParticleGizmo;

    public List<Spring> springs = new List<Spring>();
    void Awake()
    {
        //if (ParticleGizmo == null)
        //    ParticleGizmo = new GameObject();

        //determine number of grid points after subdivide
        Subdivide(divisions);
        int totalPoints = rows * cols;
        particles = new SParticle[totalPoints];

        //gizmos
        pGizmos = new GameObject[totalPoints];

        for(int i = 0; i < totalPoints; ++i)
        {
            GameObject g = Instantiate(ParticleGizmo, Vector3.zero, Quaternion.identity) as GameObject;
            g.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            g.transform.parent = gameObject.transform;
            pGizmos[i] = g;

        }

        //int totalPoints = 1;
        //for(int i = 1; i < divisions; ++i)
        //{
        //    totalPoints *= numVerts;
        //    GameObject g = new GameObject();
        //}
        //Debug.Log(totalPoints);

        //set grid points
        int iter = 0;
        for (int i = 0; i < cols; ++i)
        {
            for(int j = 0; j < rows; ++j)
            {
                SParticle p = new SParticle();
                Spring s = new Spring();

                p.position = new Vector3(j, i, 0);
                particles[iter] = p;

                //gizmos
                pGizmos[iter].transform.position = new Vector3(j, i, 0);


                //horizontal springs
                if (j != 0)
                {
                    s.p1 = particles[i * cols + j - 1];
                    s.p2 = particles[i * cols + j];
                    springs.Add(s);
                    Debug.Log("spring " + (springs.Count - 1) + " start: " + s.p1.position + " end: " + s.p2.position);
                }

                //vertical springs
                if (i != 0)
                {
                    s.p1 = particles[(i - 1) * cols + j];
                    s.p2 = particles[i * cols + j];

                    springs.Add(s);
                    Debug.Log("spring " + (springs.Count - 1) + " start: " + s.p1.position + " end: " + s.p2.position);

                }



                ////horizontal springs
                //if (j % cols != 0)
                //{
                //    s.p1 = particles[i * cols + j - 1];
                //    s.p2 = particles[i * cols + j];
                //    springs.Add(s);
                //    Debug.Log("spring " + (springs.Count-1) + " start: " + s.p1.position + " end: " + s.p2.position);
                //}

                ////vertical springs
                //if (i % cols != 0)
                //{
                //    s.p1 = particles[(i - 1) * cols + j];
                //    s.p2 = particles[i * cols + j];

                //    springs.Add(s);
                //    Debug.Log("spring " + (springs.Count - 1) + " start: " + s.p1.position + " end: " + s.p2.position);

                //}

                iter++;
               
            }
        }

        



    }

    void Update()
    {
        foreach (Spring s in springs)
        {
         //   Debug.Log("Draw Line from " + s.p1.position + " to " + s.p2.position);
            Debug.DrawLine(s.p1.position, s.p2.position, Color.white);
        }
    }


    void Subdivide(int numDivs)
    {
        for (int i = 0; i < numDivs; ++i)
        {
            rows = (rows * 2) - 1;
            cols = (cols * 2) - 1;
        }
    }

}
