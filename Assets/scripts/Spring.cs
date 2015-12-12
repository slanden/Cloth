using UnityEngine;
using System.Collections;

public enum SpringType { manhattan, structural, shear, bend };
public class Spring
{
    public SpringType springType; //type of spring
    public float spring; // stiffness
    public float damp; // damper
    public float restLength; // rest length
    public SParticle p1, p2;

    public void ComputeForce()
    {
        Vector3 dist = p2.position - p1.position;
        float force;

        //spring
        force = -spring * (restLength - Vector3.Distance(p1.position, p2.position));

        //damper
        float p1Vel = Vector3.Dot(dist.normalized, p1.velocity);
        float p2Vel = Vector3.Dot(dist.normalized, p2.velocity);

        force += (-damp * (p1Vel - p2Vel));

        Vector3 force1 = force * dist.normalized;
        Vector3 force2 = -force1;

        p1.force += force1;
        p2.force += force2;
    }
}