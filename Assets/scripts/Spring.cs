using UnityEngine;

public enum SpringType { structural, shear, bend };
public class Spring
{
    public SpringType springType; //type of spring
    public float spring; // stiffness
    public float damp; // damper
    public float restLength; // rest length
    public SParticle p1, p2;

    public void ComputeForce()
    {
        //get the distance between the particle nodes to get the direction
        Vector3 dist = p2.position - p1.position;
        float force;

        // calculate the spring force
        force = -spring * (restLength - Vector3.Distance(p1.position, p2.position));

        //calculate the damper force
        float p1Vel = Vector3.Dot(dist.normalized, p1.velocity);
        float p2Vel = Vector3.Dot(dist.normalized, p2.velocity);

        //append the damper force to the spring force
        force += (-damp * (p1Vel - p2Vel));

        //apply the force to particle 1 toward particle 2
        Vector3 force1 = force * dist.normalized;
        //apply the opposite reaction to the particle 1 force
        Vector3 force2 = -force1;

        p1.force += force1;
        p2.force += force2;
    }
}