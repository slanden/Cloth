using UnityEngine;
using System.Collections.Generic;

public class SCloth : MonoBehaviour
{
    public int divisions = 0;
    public int cols = 2;
    public int rows = 2;
	int width = 1;
	int height = 1;
    public bool setAnchorsMode = false;
	

    public float stiffness = 2;
    public float dampening = 1;
    public float restLength = 0.25f;
    public float constMass = 0.5f;

    public SParticle[] particles;
    public List<Spring> springs = new List<Spring>();
	//public List<Line> lines = new List<Line>();

    //public GameObject[] pGizmos;
    //public GameObject ParticleGizmo;    

    Vector3 gravity = new Vector3(0, -1, 0);

    void Awake()
    {
		
        //determine number of grid points after subdividing
        Subdivide(divisions);
        int totalPoints = rows * cols;
        particles = new SParticle[totalPoints];

        //set grid points
        int iter = 0;
        for (int i = 0; i < cols; ++i)
        {
            for (int j = 0; j < rows; ++j)
            {
                //create particles
                SParticle p = new SParticle();

                p.position = new Vector3(j, i, 0);
                p.mass = constMass;
                p.velocity = Vector3.zero;
                particles[iter] = p;

                ///create springs///
                //horizontal springs
                if (j != 0)
                {
                    Spring s = new Spring();
                    s.spring = stiffness;
                    s.damp = dampening;
                    s.restLength = this.restLength;

                    s.p1 = particles[i * cols + j - 1];
                    s.p2 = particles[i * cols + j];
                    springs.Add(s);
					
					/* //make line
					Line l = new Line();
					l.InitLineRenderer();
					l.AddVerts(s.p1.position,s.p2.position);
					lines.Add(l) */
                }

                //vertical springs
                if (i != 0)
                {
                    Spring s = new Spring();
                    s.spring = stiffness;
                    s.damp = dampening;
                    s.restLength = this.restLength;

                    s.p1 = particles[(i - 1) * cols + j];
                    s.p2 = particles[i * cols + j];
                    springs.Add(s);
					
					/* //make line
					Line l = new Line();
					l.InitLineRenderer();
					l.AddVerts(s.p1.position,s.p2.position);
					lines.Add(l) */
                }

                //diagonal right springs
                if (i != 0 && j != 0)
                {
                    Spring s = new Spring();
                    s.spring = stiffness;
                    s.damp = dampening;
                    s.restLength = this.restLength;

                    s.p1 = particles[(i - 1) * cols + j - 1];
                    s.p2 = particles[i * cols + j];
                    springs.Add(s);
					
					/* //make line
					Line l = new Line();
					l.InitLineRenderer();
					l.AddVerts(s.p1.position,s.p2.position);
					lines.Add(l) */
                }

                //diagonal left springs
                if (i != 0 && j != cols-1)
                {
                    Spring s = new Spring();
                    s.spring = stiffness;
                    s.damp = dampening;
                    s.restLength = this.restLength;

                    s.p1 = particles[(i - 1) * cols + j + 1];
                    s.p2 = particles[i * cols + j];
                    springs.Add(s);
					
/* 					//make line
					Line l = new Line();
					l.InitLineRenderer();
					l.AddVerts(s.p1.position,s.p2.position);
					lines.Add(l) */
                }

                iter++;
            }

        }
        //set start anchor points for testing
        particles[(totalPoints - cols)].anchor = true;
        particles[totalPoints - 1].anchor = true;
    }

    //this update is supposed to be for setting particle node anchors
    void Update()
    {
            if(setAnchorsMode)
            {
                Vector3 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                Vector3 screenPos = new Vector3(0,0,0);

                Debug.Log("mousePos: " + mousePos);
                Debug.Log("screenPos: " + screenPos);
                //if a particle is under the mouse cursor, enable particleGizmo
                foreach(SParticle p in particles)
                {
                    screenPos = Camera.main.WorldToScreenPoint(p.position);
                    Debug.Log("mousePos: " + mousePos);
                    Debug.Log("screenPos: " + screenPos);
                    if(Mathf.Abs(Vector3.Magnitude(mousePos - screenPos)) < 1f)
                    {
                        //GameObject g = Instantiate(ParticleGizmo, Vector3.zero, Quaternion.identity) as GameObject;
                        //g.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                        //g.transform.parent = gameObject.transform;
                        //g.transform.position = p.position;
                        Debug.Log(p.position);
                        Gizmos.DrawSphere(p.position, 1f);
                    }
                }                

                //if mouse click, set particle.anchor = true and disable particleGizmo
            }
    }

    void FixedUpdate()
    {
        //compute particle forces
        foreach (SParticle p in particles)
            p.force = p.mass * gravity;

        //compute spring forces
        foreach (Spring s in springs)
        {
            Debug.DrawLine(s.p1.position, s.p2.position, Color.white);
            s.ComputeForce();
        }

        //integrate motion
        for (int i = 0; i < particles.Length; ++i)
        {
            if (particles[i].anchor == false)
            {
                Vector3 acceleration = particles[i].force / particles[i].mass;
                particles[i].velocity += acceleration * Time.fixedDeltaTime;
                particles[i].position += particles[i].velocity * Time.fixedDeltaTime;
            }
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