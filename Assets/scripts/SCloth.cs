using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
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

    public Slider stiffnessSlider, damperSlider, restLengthSlider;//, massSlider;
    public Toggle AnchorModeToggle;

    public SParticle[] particles;
    public List<Spring> springs = new List<Spring>();
    public List<LineRenderer> springsLines = new List<LineRenderer>();
    //public List<Line> lines = new List<Line>();
    public GameObject line;
    public GameObject particleGizmo;

    Vector3 gravity = new Vector3(0, -1, 0);


    void Awake()
    {
        if (particleGizmo == null)
        {
            particleGizmo = new GameObject();
            Debug.Log("particleGizmo not set");
        }
        particleGizmo = Instantiate(particleGizmo) as GameObject;
        particleGizmo.name = "NodeMesh";
        particleGizmo.transform.parent = transform;
        particleGizmo.SetActive(false);

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
                }

                //diagonal left springs
                if (i != 0 && j != cols - 1)
                {
                    Spring s = new Spring();
                    s.spring = stiffness;
                    s.damp = dampening;
                    s.restLength = this.restLength;

                    s.p1 = particles[(i - 1) * cols + j + 1];
                    s.p2 = particles[i * cols + j];
                    springs.Add(s);
                }

                iter++;
            }

        }
        //set start anchor points for testing
        particles[(totalPoints - cols)].anchor = true;
        particles[totalPoints - 1].anchor = true;
    }

    void Start()
    {
        stiffnessSlider.value = stiffness;
        damperSlider.value = dampening;
        restLengthSlider.value = restLength;
        AnchorModeToggle.isOn = setAnchorsMode;
        //massSlider.value = constMass;

        int count = 0;
        GameObject sparent = new GameObject();
        sparent.name = "Springs";
        foreach (Spring s in springs)
        {
            GameObject g = Instantiate(line);
            g.name = "Spring " + count.ToString();
            g.transform.SetParent(sparent.transform);
            LineRenderer l = g.GetComponent<LineRenderer>();
            l.SetPosition(0, s.p1.position);
            l.SetPosition(1, s.p2.position);
            springsLines.Add(l);
            count++;
        }
    }

    void OnGUI()
    {
        stiffness = stiffnessSlider.value;
        dampening = damperSlider.value;
        restLength = restLengthSlider.value;
        //constMass = massSlider.value;
        setAnchorsMode = AnchorModeToggle.isOn;
    }

    void FixedUpdate()
    {
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y,0);
        //Vector3 mousePos = Input.mousePosition;

        //compute particle forces
        foreach (SParticle p in particles)
        {
            p.force = p.mass * gravity;

            Vector3 screenPoint = Camera.main.WorldToScreenPoint(p.position);

            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 offset = p.position - Camera.main.ScreenToWorldPoint(curScreenPoint);

            //Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

            //Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
            //p.position = curPosition;


            Vector3 nsPos = Camera.main.WorldToScreenPoint(p.position);
            Vector2 nodeScreenPos = new Vector3(nsPos.x, nsPos.y,0);
            

            if (Vector3.Distance(p.position, Camera.main.ScreenToWorldPoint(curScreenPoint) ) < 4f )
            {
                particleGizmo.SetActive(true);
                
                if (Input.GetMouseButton(0))
                {
                    p.anchor = true;
                    //p.position = Camera.main.ScreenToWorldPoint(mousePos);
                    Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
                    p.position = curPosition;
                    p.anchor = false;
                }
                particleGizmo.transform.position = p.position;

                if(Input.GetMouseButtonDown(1))
                    p.anchor = true;

                break;
            }
            else
                particleGizmo.SetActive(false);

            
                
        }
            


        int e = 0;
        //compute spring forces
        foreach (Spring s in springs)
        {
            s.spring = stiffness;
            s.damp = dampening;
            s.restLength = this.restLength;
            //Debug.DrawLine(s.p1.position, s.p2.position, Color.white);
            springsLines[e].SetPosition(0, s.p1.position);
            springsLines[e].SetPosition(1, s.p2.position);
            s.ComputeForce();
            e++;
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

        //manipulate nodes
        //if(Input.GetMouseButton(0))
        //{

        //}

        //if (setAnchorsMode)
        //{
        //    Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        //    foreach (SParticle p in particles)
        //    {
        //        Vector3 nsPos = Camera.main.WorldToScreenPoint(p.position);
        //        Vector2 nodeScreenPos = new Vector2(nsPos.x, nsPos.y);

        //        if (Vector3.Distance(nodeScreenPos, mousePos) < 4f)
        //        {
        //            particleGizmo.SetActive(true);
        //            particleGizmo.transform.position = p.position;
        //            if (Input.GetMouseButtonDown(0))
        //                p.anchor = true;
        //            break;
        //        }
        //        else
        //            particleGizmo.SetActive(false);
        //    }

        //}

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