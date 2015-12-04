using UnityEngine;
using System.Collections;

public class SParticle : MonoBehaviour 
{
    public Vector3 position;
    public float velocity;
    public Vector3 acceleration;
    public float mass;
    public float momentum;
    public Vector3 force;
    public bool anchor = false;

    public void ApplyGravity()
    {
        //gravity
        acceleration = new Vector3(0, -9.8f, 0);

        force = mass * acceleration;
    }
}
