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

    
    public float stiffness = 1;
    public float dampening = 1;
    public float restLength = 1;
    public float constMass = 0.5f;

    public float drag = 0.5f;
    public float density = 1f;

    public Slider stiffnessSlider, damperSlider, restLengthSlider;//, massSlider;
    public Toggle AnchorModeToggle;

    public SParticle[] particles;
    public List<Spring> springs = new List<Spring>();
    public List<LineRenderer> springsLines = new List<LineRenderer>();
    public List<Triangle> triangles = new List<Triangle>();
    

    public GameObject line;
    public GameObject particleGizmo;
    

    Vector3 gravity = new Vector3(0, -1, 0);
    Vector3 airVelocity = new Vector3(1, 0, 0);


    public float manhattanSpringConst;
    public float manhattanSpringRestLength;
    public float manhattanDampConst;

    public float structuralSpringConst;
    public float structuralSpringRestLength;
    public float structuralDampConst;

    public float shearSpringConst;
    public float shearSpringRestLength;
    public float shearDampConst;

    public float bendSpringConst;
    public float bendSpringRestLength;
    public float bendDampConst;

    //public GUIText airVelocityX;
    //public GUIText airVelocityY;
    //public GUIText airVelocityZ;


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
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < cols; ++j)
            {
                //create particles
                SParticle p = new SParticle();

                p.position = new Vector3(transform.position.x + j, transform.position.y + i, transform.position.z + 0);
                p.mass = constMass;
                p.velocity = Vector3.zero;
                particles[iter] = p;

                ///create springs///
                //horizontal springs
                if (j != 0)
                {
                    Spring s = new Spring();
                    s.springType = SpringType.manhattan;
                    s.spring = stiffness;
                    s.damp = dampening;
                    s.restLength = this.restLength;

                    s.p1 = particles[i * cols + j - 1];
                    s.p2 = particles[i * cols + j];
                    springs.Add(s);
                }

                ////horizontal bend springs
                //if (j != 0)
                //{
                //    Spring s = new Spring();
                //    s.springType = SpringType.bend;
                //    s.spring = stiffness;
                //    s.damp = dampening;
                //    s.restLength = this.restLength;

                //    s.p1 = particles[i * cols + j - 1];
                //    s.p2 = particles[i * cols + j];
                //    springs.Add(s);
                //}

                //vertical springs
                if (i != 0)
                {
                    Spring s = new Spring();
                    s.springType = SpringType.manhattan;
                    s.spring = stiffness;
                    s.damp = dampening;
                    s.restLength = this.restLength;

                    s.p1 = particles[(i - 1) * cols + j];
                    s.p2 = particles[i * cols + j];
                    springs.Add(s);
                }

                ////vertical bend springs
                //if (i != 0)
                //{
                //    Spring s = new Spring();
                //    s.springType = SpringType.bend;
                //    s.spring = stiffness;
                //    s.damp = dampening;
                //    s.restLength = this.restLength;

                //    s.p1 = particles[(i - 1) * cols + j];
                //    s.p2 = particles[i * cols + j];
                //    springs.Add(s);
                //}

                //diagonal up right springs  --Matthews up springs
                if (i != 0 && j != 0)
                {
                    Spring s = new Spring();
                    s.springType = SpringType.shear;
                    s.spring = stiffness;
                    s.damp = dampening;
                    s.restLength = this.restLength;

                    s.p1 = particles[(i - 1) * cols + j - 1];
                    s.p2 = particles[i * cols + j];
                    springs.Add(s);
                }

                //diagonal up left springs  --Matthew's down springs
                if (i != 0 && j != cols - 1)
                {
                    Spring s = new Spring();
                    s.springType = SpringType.structural;
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


        //create triangles
        for (int i = 0; i < totalPoints; ++i)
        {
            if ((i < totalPoints - rows) && ((i % rows) != (rows - 1)))
            {
                Triangle t1 = new Triangle();
                Triangle t2 = new Triangle();
                Triangle t3 = new Triangle();
                Triangle t4 = new Triangle();
                //NW, NE, SW
                t1.p1 = particles[i];
                t1.p2 = particles[i + 1];
                t1.p3 = particles[i + cols];

                //NW, NE, SE
                t2.p1 = particles[i];
                t2.p2 = particles[i + 1];
                t2.p3 = particles[i + cols + 1];

                //NE, SW, SE
                t3.p1 = particles[i + 1];
                t3.p2 = particles[i + cols];
                t3.p3 = particles[i + cols + 1];

                //NW, SW, SE
                t4.p1 = particles[i];
                t4.p2 = particles[i + cols];
                t4.p3 = particles[i + cols + 1];

                triangles.Add(t1);
                triangles.Add(t2);
                triangles.Add(t3);
                triangles.Add(t4);
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

        //airVelocity.x = GUI.HorizontalSlider(new Rect(Screen.width / 2 + 200, Screen.height - 150, 200, 30), airVelocity.x, -50.0f, 50.0f);
        //airVelocity.y = GUI.HorizontalSlider(new Rect(Screen.width / 2 + 200, Screen.height - 100, 200, 30), airVelocity.y, -50.0f, 50.0f);
        //airVelocity.z = GUI.HorizontalSlider(new Rect(Screen.width / 2 + 200, Screen.height - 50, 200, 30), airVelocity.z, -50.0f, 50.0f);

        //Vector3 newAirVX = new Vector3(0.5f, 0.2f);
        //Vector3 newAirVY = new Vector3(0.5f, 0.12f);
        //Vector3 newAirVZ = new Vector3(0.5f, 0.05f);

        //airVelocityX.transform.position = newAirVX;
        //airVelocityY.transform.position = newAirVY;
        //airVelocityZ.transform.position = newAirVZ;

        //airVelocityX.text = "Air velocity X: " + airVelocity.x.ToString();
        //airVelocityY.text = "Air velocity Y: " + airVelocity.y.ToString();
        //airVelocityZ.text = "Air velocity Z: " + airVelocity.z.ToString();
    }

    void FixedUpdate()
    {
        //compute particle forces
        foreach (SParticle p in particles)
            p.force = p.mass * gravity;


        int e = 0;
        //compute spring forces
        foreach (Spring s in springs)
        {
            switch (s.springType)
            {
                case SpringType.bend:
                    s.spring = bendSpringConst * stiffness;
                    s.damp = bendDampConst * dampening;
                    restLength = bendSpringRestLength * this.restLength;
                    break;
                case SpringType.manhattan:
                    s.spring = manhattanSpringConst * stiffness;
                    s.damp = manhattanDampConst * dampening;
                    restLength = manhattanSpringRestLength * this.restLength;
                    break;
                case SpringType.shear:
                    s.spring = shearSpringConst * stiffness;
                    s.damp = shearDampConst * dampening;
                    restLength = shearSpringRestLength * this.restLength;
                    break;
                case SpringType.structural:
                    s.spring = structuralSpringConst * stiffness;
                    s.damp = structuralDampConst * dampening;
                    restLength = structuralSpringRestLength * this.restLength;
                    break;
            }

            springsLines[e].SetPosition(0, s.p1.position);
            springsLines[e].SetPosition(1, s.p2.position);
            s.ComputeForce();
            e++;
        }


        //compute aerodynamic forces
        foreach (Triangle t in triangles)
        {
            Vector3 velocity = (t.p1.velocity + t.p2.velocity + t.p3.velocity) / 3f;
            velocity -= airVelocity;

            Vector3 crossAB = Vector3.Cross((t.p2.position - t.p1.position), (t.p3.position - t.p1.position));
            Vector3 normal = crossAB / Vector3.Magnitude(crossAB);

            //what is this?
            Vector3 aeroForce = -0.5f * drag * density * ((0.5f * Vector3.Dot(velocity, normal) * velocity.magnitude) / crossAB.magnitude) * crossAB;

            aeroForce /= 3f;

            t.p1.force += aeroForce;
            t.p2.force += aeroForce;
            t.p3.force += aeroForce;
        }


        bool gizmoActive = particleGizmo.activeSelf;
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);


        //integrate motion
        for (int i = 0; i < particles.Length; ++i)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(particles[i].position);
            mousePos.z = screenPoint.z;

            if (Vector3.SqrMagnitude(screenPoint - mousePos) < 4f * 4f)
            {
                particleGizmo.SetActive(true);
                particleGizmo.transform.position = particles[i].position;
                Debug.Log("particle " + i);

                if(Input.GetMouseButton(0) && particles[i].animated != true)
                {
                    particles[i].animated = true;
                }
                else if (Input.GetMouseButton(0) && particles[i].animated == true)
                {
                    Vector3 direction;
                    direction = Camera.main.ScreenToWorldPoint(mousePos - screenPoint);
                    particles[i].force += direction;
                }

            }
            else if (gizmoActive && !Input.GetMouseButton(0))
            {
                particleGizmo.SetActive(false);
                gizmoActive = false;
            }

            if (!Input.GetMouseButton(0))
                particles[i].animated = false;

            //integrate motion (cont.)
            if (particles[i].anchor == false)
            {
                Vector3 acceleration = particles[i].force / particles[i].mass;
                particles[i].velocity += acceleration * Time.fixedDeltaTime;
                particles[i].position += particles[i].velocity * Time.fixedDeltaTime;
            }
        }
        gizmoActive = particleGizmo.activeSelf;

    }

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


    void Subdivide(int numDivs)
    {
        for (int i = 0; i < numDivs; ++i)
        {
            rows = (rows * 2) - 1;
            cols = (cols * 2) - 1;
        }
    }

}



//if (Input.GetMouseButton(0) && particleGizmo.activeSelf)
//{
//    particleGizmo.transform.position = Camera.main.ScreenToWorldPoint(mousePos);
//    //p.force += Vector3.Normalize(particleGizmo.transform.position - p.position) * 2;
//    //p.position = Vector3.Lerp(p.position, particleGizmo.transform.position, Time.fixedDeltaTime);
//}




//if (Input.GetMouseButton(0))
//{
//    particleGizmo.transform.position = Camera.main.ScreenToWorldPoint(mousePos);
//    p.force += Vector3.Normalize(particleGizmo.transform.position - p.position) * Time.fixedDeltaTime;
//}


//if (Input.GetMouseButtonDown(1))
//    p.anchor = true;