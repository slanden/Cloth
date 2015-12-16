using UnityEngine;
using UnityEngine.UI;
//using UnityEditor;
using System.Collections.Generic;

public class SCloth : MonoBehaviour
{
    public int divisions = 0;
    public int cols = 2;
    public int rows = 2;
    int width = 1;
    int height = 1;
    
    public float stiffness = 1;
    public float dampening = 1;
    public float restLength = 1;
    public float constMass = 0.5f;

    public float drag = 0.5f;
    public float density = 1f;

    public Slider stiffnessSlider, damperSlider;//, restLengthSlider;//, massSlider;
    public Slider windSliderX, windSliderY, windSliderZ;

    public SParticle[] particles;
    public List<Spring> springs = new List<Spring>();
    public List<LineRenderer> springsLines = new List<LineRenderer>();
    public List<Triangle> triangles = new List<Triangle>();
    

    public GameObject line;
    public GameObject particleGizmo;
    

    Vector3 gravity = new Vector3(0, -9.8f, 0);
    public Vector3 airVelocity = new Vector3(1, 1, -5);

    public float structuralSpringConst;
    public float structuralSpringRestLength;
    public float structuralDampConst;

    public float shearSpringConst;
    public float shearSpringRestLength;
    public float shearDampConst;

    public float bendSpringConst;
    public float bendSpringRestLength;
    public float bendDampConst;
    public GameObject cutter;


    void Awake()
    {
        //instantiate particle gizmo
        if (particleGizmo == null)
        {
            particleGizmo = new GameObject();
            Debug.Log("particleGizmo not set");
        }
        particleGizmo = Instantiate(particleGizmo) as GameObject;
        particleGizmo.name = "NodeMesh";
        particleGizmo.transform.parent = transform;
        particleGizmo.SetActive(false);

        //instantiate cutter
        cutter = Instantiate(cutter) as GameObject;
        cutter.name = "Cutter";
        cutter.transform.parent = transform;
        Vector3 offscreen = new Vector3(-100, 0, 0);
        offscreen = Camera.main.ScreenToWorldPoint(offscreen);
        cutter.transform.position = offscreen;

        InitCloth();
    }

    void Start()
    {
        //set UI elements
        stiffnessSlider.value = stiffness;
        damperSlider.value = dampening;
        //restLengthSlider.value = restLength;

        windSliderX.value = airVelocity.x;
        windSliderY.value = airVelocity.y;
        windSliderZ.value = airVelocity.z;
        //massSlider.value = constMass;

        //make render lines
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
        //restLength = restLengthSlider.value;

        airVelocity.x = windSliderX.value;
        airVelocity.y = windSliderY.value;
        airVelocity.z = windSliderZ.value;
        //constMass = massSlider.value;
    }

    void FixedUpdate()
    {
        bool gizmoActive = particleGizmo.activeSelf;
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);

        //compute particle forces
        foreach (SParticle p in particles)
            p.force = p.mass * gravity;

        //tearing
        if (Input.GetMouseButton(1))
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(cutter.transform.position);
            Vector3 mouseWorldPos = mousePos;
            mouseWorldPos.z = screenPoint.z;
            mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseWorldPos);
            cutter.transform.position = mouseWorldPos;
        }


        //compute spring forces
        int e = 0;
        foreach (Spring s in springs)
        {
            switch (s.springType)
            {
                case SpringType.structural:
                    s.spring = structuralSpringConst * stiffness;
                    s.damp = structuralDampConst * dampening;
                    s.restLength = structuralSpringRestLength * this.restLength;
                    break;
                case SpringType.shear:
                    s.spring = shearSpringConst * stiffness;
                    s.damp = shearDampConst * dampening;
                    s.restLength = shearSpringRestLength * this.restLength;
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
            //get the average velocity of the triangle points
            Vector3 velocity = (t.p1.velocity + t.p2.velocity + t.p3.velocity) / 3f;
            //subtract the air velocity
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

        
        //integrate motion and input checks
        for (int i = 0; i < particles.Length; ++i)
        {
            //convert the position of the particle to screen space
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(particles[i].position);
            //set the mouse position depth to the same as the particle position depth in screen coords
            mousePos.z = screenPoint.z;
            //get the distance squared to avoid the square root as it's not needed for this purpose
            float distanceSqrd = Vector3.SqrMagnitude(screenPoint - mousePos);

            //square the right hand side and do the distance check
            if (distanceSqrd < 4f * 4f)
            {
                particleGizmo.SetActive(true);
                particleGizmo.transform.position = particles[i].position;

                if (Input.GetMouseButton(0) && particles[i].animated != true)
                {
                    particles[i].animated = true;
                }

            }
            else if (gizmoActive && !Input.GetMouseButton(0))
            {
                particleGizmo.SetActive(false);
                gizmoActive = false;
            }
            
            //dragging
            if (Input.GetMouseButton(0) && particles[i].animated == true)
            {
                Vector3 direction;
                direction = Camera.main.ScreenToWorldPoint(mousePos - screenPoint);
                Vector3 newMousePos = new Vector3(mousePos.x, mousePos.y, screenPoint.z);
                //particles[i].position = Camera.main.ScreenToWorldPoint(newMousePos);
                particles[i].force += (Camera.main.ScreenToWorldPoint(newMousePos) - particles[i].position) * 800;
            }

            //set the particle to not being moved while the mouse button is no longer being held down
            if (!Input.GetMouseButton(0))
                particles[i].animated = false;

            //anchoring
            if (Input.GetKeyDown(KeyCode.A) && particles[i].animated || 
                Input.GetKeyDown(KeyCode.A) && distanceSqrd < 4f * 4f )
            {
                if (particles[i].anchor == true)
                    particles[i].anchor = false;
                else
                    particles[i].anchor = true;
            }


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


    void Subdivide(int numDivs)
    {
        for (int i = 0; i < numDivs; ++i)
        {
            rows = (rows * 2) - 1;
            cols = (cols * 2) - 1;
        }
    }

    public void InitCloth()
    {
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
                    s.springType = SpringType.structural;
                    s.spring = structuralSpringConst;
                    s.damp = structuralDampConst;
                    s.restLength = structuralSpringRestLength;

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
                    s.springType = SpringType.structural;
                    s.spring = structuralSpringConst;
                    s.damp = structuralDampConst;
                    s.restLength = structuralSpringRestLength;

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

                //diagonal up right springs
                if (i != 0 && j != 0)
                {
                    Spring s = new Spring();
                    s.springType = SpringType.shear;
                    s.spring = shearSpringConst;
                    s.damp = shearDampConst;
                    s.restLength = shearSpringRestLength;

                    s.p1 = particles[(i - 1) * cols + j - 1];
                    s.p2 = particles[i * cols + j];
                    springs.Add(s);
                }

                //diagonal up left springs
                if (i != 0 && j != cols - 1)
                {
                    Spring s = new Spring();
                    s.springType = SpringType.shear;
                    s.spring = shearSpringConst;
                    s.damp = shearDampConst;
                    s.restLength = shearSpringRestLength;

                    s.p1 = particles[(i - 1) * cols + j + 1];
                    s.p2 = particles[i * cols + j];
                    springs.Add(s);
                }

                iter++;
            }

        }

        #region more spring stuff
        ////bend springs
        //Spring top = new Spring();
        //Spring bottom = new Spring();
        //Spring left = new Spring();
        //Spring right = new Spring();

        ////horizontal spring length

        //float hBendSpringRestLength = particles[(totalPoints - 1)].position.x -
        //                              particles[totalPoints - cols].position.x;

        //float vBendSpringRestLength = particles[totalPoints - cols].position.y -
        //                              particles[0].position.y;

        //top.p1 = particles[(totalPoints - cols)];
        //top.p2 = particles[totalPoints - 1];
        //top.springType = SpringType.bend;
        //top.spring = stiffness;
        //top.damp = dampening;
        //top.restLength = hBendSpringRestLength;

        //bottom.p1 = particles[0];
        //bottom.p2 = particles[cols - 1];
        //bottom.springType = SpringType.bend;
        //bottom.spring = stiffness;
        //bottom.damp = dampening;
        //bottom.restLength = hBendSpringRestLength;

        //left.p1 = particles[0];
        //left.p2 = particles[totalPoints - cols];
        //left.springType = SpringType.bend;
        //left.spring = stiffness;
        //left.damp = dampening;
        //left.restLength = vBendSpringRestLength;

        //right.p1 = particles[cols - 1];
        //right.p2 = particles[totalPoints - 1];
        //right.springType = SpringType.bend;
        //right.spring = stiffness;
        //right.damp = dampening;
        //right.restLength = vBendSpringRestLength;

        //springs.Add(top);
        //springs.Add(bottom);
        //springs.Add(left);
        //springs.Add(right);
        #endregion

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

        //set start anchor points
        particles[(totalPoints - cols)].anchor = true;
        particles[totalPoints - 1].anchor = true;
    }


}