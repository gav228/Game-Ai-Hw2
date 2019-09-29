using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCController : MonoBehaviour {


    // Store variables for objects
    private SteeringBehavior ai;    // Put all the brains for steering in its own module
    private Rigidbody rb;           // You'll need this for dynamic steering

    // For speed 
    public Vector3 position;        // local pointer to the RigidBody's Location vector
    public Vector3 velocity;        // Will be needed for dynamic steering

    // For rotation
    public float orientation;       // scalar float for agent's current orientation
    public float rotation;          // Will be needed for dynamic steering

    public float maxSpeed;          // what it says

    public int phase;               // use this to control which "phase" the demo is in

    private Vector3 linear;         // The resilts of the kinematic steering requested
    private float angular;          // The resilts of the kinematic steering requested

    public Text label;              // Used to displaying text nearby the agent as it moves around
    LineRenderer line;              // Used to draw circles and other things

    private void Start() {
        ai = GetComponent<SteeringBehavior>();
        rb = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();
        position = rb.position;
        orientation = transform.eulerAngles.y;
    }

    /// <summary>
    /// Depending on the phase the demo is in, have the agent do the appropriate steering.
    /// 
    /// </summary>
    void FixedUpdate() {
        switch (phase) {
            case 1:

                // Pursue

                // Assign algorithm label text
                if (label) { 
                    label.text = name.Replace("(Clone)","") + "\nAlgorithm: Smarter Pursue"; 
                }

                // Get linear acceleration
                linear = ai.Pursue();
                linear = ai.WallAvoidance(linear);
                // Get angular accelleration
                angular = ai.Face();    

                break;

            case 2:

                // Arrive 

                // Assign algorithm label text
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Smarter Arrive";
                }

                // Get linear acceleration
                linear = ai.Arrive();
                linear = ai.WallAvoidance(linear);
                // Get angular accelleration
                angular = ai.Face();
                
                break;

            case 3:

                // Evade

                // Assign algorithm label text
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Smarter Evade";
                }

                // Get linear acceleration
                linear = ai.Evade();
                linear = ai.WallAvoidance(linear);
                // Get angular acceleration
                angular = ai.Face_Where_Im_Going(linear);
                break;

            case 4:
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Smarter Wander algorithm";
                }

                angular = ai.Wander(out linear);
                
                //linear = ai.WallAvoidance(linear);
                //angular = ai.Face_Where_Im_Going(linear);

                break;
            case 5:
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Wall Avoidance algorithm";
                }

                linear = ai.Pursue();
                linear = ai.WallAvoidance(linear);
                angular = ai.Face_Where_Im_Going(linear);
                
                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;

            case 6:
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Collision Detection algorithm";
                }

                linear = ai.Pursue();
                linear = ai.WallAvoidance(linear);
                angular = ai.Face_Where_Im_Going(linear);

                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;
            case 7:
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Collision Prediction algorithm";
                }

                linear = ai.Pursue();
                linear = ai.WallAvoidance(linear);
                angular = ai.Face_Where_Im_Going(linear);

                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;
            case 8:
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Chase the player algorithm";
                }

                linear = ai.ChasePlayer();
                linear = ai.WallAvoidance(linear);
                angular = ai.Face_Where_Im_Going(linear);

                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;

        }
        update(linear, angular, Time.deltaTime);
        if (label) {
            label.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);
        }
    }

    private void update(Vector3 steeringlin, float steeringang, float time) {
        // Update the orientation, velocity and rotation
        orientation += rotation * time;
        velocity += steeringlin * time;
        rotation += steeringang * time;

        if (velocity.magnitude > maxSpeed) {
            velocity.Normalize();
            velocity *= maxSpeed;
        }

        rb.AddForce(velocity - rb.velocity, ForceMode.VelocityChange);
        position = rb.position;
        rb.MoveRotation(Quaternion.Euler(new Vector3(0, Mathf.Rad2Deg * orientation, 0)));
    }

    // <summary>
    // The next two methods are used to draw circles in various places as part of demoing the
    // algorithms.

    /// <summary>
    /// Draws a circle with passed-in radius around the center point of the NPC itself.
    /// </summary>
    /// <param name="radius">Desired radius of the concentric circle</param>
    public void DrawConcentricCircle(float radius) {
        line.positionCount = 51;
        line.useWorldSpace = false;
        float x;
        float z;
        float angle = 20f;

        for (int i = 0; i < 51; i++) {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            line.SetPosition(i, new Vector3(x, 0, z));
            angle += (360f / 51);
        }
    }

    /// <summary>
    /// Draws a circle with passed-in radius and arbitrary position relative to center of
    /// the NPC.
    /// </summary>
    /// <param name="position">position relative to the center point of the NPC</param>
    /// <param name="radius">>Desired radius of the circle</param>
    public void DrawCircle(Vector3 position, float radius) {
        line.positionCount = 51;
        line.useWorldSpace = true;
        float x;
        float z;
        float angle = 20f;

        for (int i = 0; i < 51; i++) {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            line.SetPosition(i, new Vector3(x, 1, z)+position);
            angle += (360f / 51);
        }
    }

    public void DestroyPoints() {
        if (line) {
            line.positionCount = 0;
        }
    }
}
