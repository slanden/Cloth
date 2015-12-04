using UnityEngine;
using System.Collections;

public class Spring : MonoBehaviour
{
    public float spring; // stiffness
    public float damp; // damper
    public float restLength; // rest length
    public SParticle p1, p2;

    void ComputeForce()
    {
        Vector3 dist = p2.position - p1.position;
        float force;
        //spring
        force = -spring * (restLength - dist.magnitude);

        //damper
        damp = -damp * (p1.velocity - p2.velocity);

        force += damp;

        Vector3 force1 = force * dist.normalized;
        Vector3 force2 = -force1;

        p1.force += force1;
        p2.force += force2;
    }
}
