using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the place to put all of the various steering behavior methods we're going
/// to be using. Probably best to put them all here, not in NPCController.
/// </summary>

public class SteeringBehavior : MonoBehaviour {

    // The agent at hand here, and whatever target it is dealing with
    public NPCController agent;
    public NPCController target;

    public GameObject Player;

    // Below are a bunch of variable declarations that will be used for the next few
    // assignments. Only a few of them are needed for the first assignment.

    // For pursue and evade functions
    public float maxPrediction;
    public float maxAcceleration;

    // For arrive function
    public float maxSpeed;
    public float targetRadiusL;
    public float slowRadiusL;
    public float timeToTarget;

    // For Face function
    public float maxRotation;
    public float maxAngularAcceleration;
    public float targetRadiusA;
    public float slowRadiusA;

    // For wander function
    public float wanderOffset;
    public float wanderRadius;
    public float wanderRate;
    private float wanderOrientation;

    // For wall avoidance
    public float avoidDistance;
    public float lookAhead;

    // Holds the path to follow
    public GameObject[] Path;
    public int current = 0;

    //Private variable for collision
    public GameObject LINE; 

    private bool collided = false;
    private string collidedN = "";

    protected void Start() {
        agent = GetComponent<NPCController>();
        wanderOrientation = agent.orientation;
        timeToTarget = 3.0f;
        maxPrediction = 1f;
        wanderRadius = 30f;
        wanderOffset = 1f;
        wanderRate = 2f;
        maxAcceleration = 50f;
        lookAhead = 0.1f;
        avoidDistance = 3f;
        maxRotation = 10f;
    }

    // Maps roation to -pi, pi interval
    public float mapToRange(float rotation)
    {
        while (rotation > Mathf.PI)
        {
            rotation -= 2 * Mathf.PI;
        }
        while (rotation < -Mathf.PI)
        {
            rotation += 2 * Mathf.PI;
        }
        return rotation;
    }

    // Basic seek algorithm
    public Vector3 Seek()
    {
        return target.position - agent.position;
    }

    // Basic flee algorithm
    public Vector3 Flee()
    {
        return agent.position - target.position;
    }

    // Arrive 
    public Vector3 Arrive()
    {
        // Check for having arrived
        if (target.position[0] - agent.position[0] < 1 && target.position[2] - agent.position[2] < 1)
        {
            agent.velocity = new Vector3(0, 0, 0);
            return new Vector3(0, 0, 0);
        }

        return (target.position - agent.position) / timeToTarget;
    }

    public Vector3 Pursue()
    {
        
        Vector3 predicted_position = target.position;

        // Distance to target
        Vector3 direction = target.position - agent.position;
        float distance = direction.sqrMagnitude;


        // Find current speed
        float speed = agent.velocity.sqrMagnitude;

        // Create a prediction
        float prediction = 0;
        if (speed <= (distance / maxPrediction))
        {
            prediction = maxPrediction;
        }
        else
        {
            prediction = distance / speed;
        }

        // Add the target's current speed to prediction
        predicted_position += target.velocity * prediction;

        // Draw the predicted location
        agent.DrawCircle(predicted_position, 0.5f);

        // Return vector to predicted location
        return predicted_position - agent.position;
    }

    public Vector3 Evade()
    {
        
        Vector3 predicted_position = target.position;

        // Distance to target
        Vector3 direction = target.position - agent.position;
        float distance = direction.sqrMagnitude;


        // Find current speed
        float speed = agent.velocity.sqrMagnitude;

        // Create Prediction
        float prediction = 0;
        if (speed <= (distance / maxPrediction))
        {
            prediction = maxPrediction;
        }
        else
        {
            prediction = distance / speed;
        }

        // Add the target's current speed to prediction
        predicted_position += target.velocity * prediction;

        // Draw the predicted position
        agent.DrawCircle(predicted_position, 0.5f);

        // Return vector away from predicted location
        return agent.position - predicted_position;
    }

    // Align algorithm 
    public float Align()
    {
        
        // Get the naive direction to the target
        float rotation = target.orientation - agent.orientation;

        // Map the result to the (-pi, pi) interval
        rotation = mapToRange(rotation);

        float rotationSize = Mathf.Abs(rotation);

        // Check if we are there, return no steering
        if (rotationSize < targetRadiusA)
        {
            agent.rotation = 0;
            return 0;
        }

        float targetRotation = maxRotation;

        // The final target rotation 
        targetRotation *= rotation / rotationSize;

        // Acceleration tries to get to the target rotation
        float angular = targetRotation - agent.rotation;
        angular /= timeToTarget;

        // Check if the acceleration is too great
        float angularAcceleration = Mathf.Abs(angular);
        if (angularAcceleration > maxAngularAcceleration)
        {
            angular /= angularAcceleration;
            angular *= maxAngularAcceleration;
        }

        return angular;

    }

    // Face Algorithm based on align algorithm but using position instead of orientation
    public float Face()
    {
        Vector3 direction = target.position - agent.position;

        // Check for a zero direction, and make no change if so
        if (direction.magnitude == 0)
        {
            return 0;
        }

        // Get the naive direction to the target
        float rotation = Mathf.Atan2(direction.x, direction.z) - agent.orientation;

        // Map the result to the (-pi, pi) interval
        rotation = mapToRange(rotation);

        float rotationSize = Mathf.Abs(rotation);

        // Check if we are there, return no steering
        if (rotationSize < targetRadiusA)
        {
            agent.rotation = 0;
            return 0;
        }

        float targetRotation = maxRotation;

        // The final target rotation 
        targetRotation *= rotation / rotationSize;

        // Acceleration tries to get to the target rotation
        float angular = targetRotation - agent.rotation;
        angular /= timeToTarget;

        // Check if the acceleration is too great
        float angularAcceleration = Mathf.Abs(angular);
        if (angularAcceleration > maxAngularAcceleration)
        {
            angular /= angularAcceleration;
            angular *= maxAngularAcceleration;
        }

        return angular;
    }


    // Uses face algorithm on where we are already heading
    public float Face_Where_Im_Going(Vector3 linear)
    {
        Vector3 direction = linear;

        // Check for a zero direction, and make no change if so
        if (direction.magnitude == 0)
        {
            return 0;
        }

        // Get the naive direction to the target
        float rotation = Mathf.Atan2(direction.x, direction.z) - agent.orientation;

        // Map the result to the 
        rotation = mapToRange(rotation);
        float rotationSize = Mathf.Abs(rotation);

        // Check if we are there, return no steering
        if (rotationSize < targetRadiusA)
        {
            agent.rotation = 0;
        }

        float targetRotation = maxRotation;

        // The final target rotation 
        targetRotation *= rotation / rotationSize;

        // Acceleration tries to get to the target rotation
        float angular = targetRotation - agent.rotation;
        angular /= timeToTarget;

        // Check if the acceleration is too great
        float angularAcceleration = Mathf.Abs(angular);
        if (angularAcceleration > maxAngularAcceleration)
        {
            angular /= angularAcceleration;
            angular *= maxAngularAcceleration;
        }

        return angular;
    }

    
    // Uses orientation and face algorithm with a splash of collision
    public float Wander(out Vector3 linear)
    {
        // Update the wander orientation
        wanderOrientation += (Random.value - Random.value) * wanderRate;

        // Calculate the combined target orientation
        float targetOrientation = wanderOrientation + agent.orientation;

        // Calculate the center of the wander circle                // character.orientation.asVector()
        Vector3 target = agent.position + wanderOffset * new Vector3(Mathf.Sin(agent.orientation), 0, Mathf.Cos(agent.orientation));
        

        // Calculate the target location                // targetOrientation.asVector()
        target += wanderRadius * new Vector3(Mathf.Sin(targetOrientation), 0, Mathf.Cos(targetOrientation));

        // Use the same algorithm as face but with new target

        // Work out the direction to target
        Vector3 direction = target - agent.position;

        // Check for a zero direction, and make no change if so
        if (direction.magnitude == 0)
        {
            linear = new Vector3(0, 0, 0); 
            return 0;
        }

        // Get the naive direction to the target
        float rotation = Mathf.Atan2(direction.x, direction.z) - agent.orientation;

        // Map the result to the 
        rotation = mapToRange(rotation);
        float rotationSize = Mathf.Abs(rotation);

        // Check if we are there, return no steering
        if (rotationSize < targetRadiusA)
        {
            agent.rotation = 0;
        }

        float targetRotation = maxRotation;

        // The final target rotation 
        targetRotation *= rotation / rotationSize;

        // Acceleration tries to get to the target rotation
        float angular = targetRotation - agent.rotation;
        angular /= timeToTarget;

        // Check if the acceleration is too great
        float angularAcceleration = Mathf.Abs(angular);
        if (angularAcceleration > maxAngularAcceleration)
        {
            angular /= angularAcceleration;
            angular *= maxAngularAcceleration;
        }

        // Now set the linear acceleration to be at full acceleration in the direction of the orientation
        linear = maxAcceleration * new Vector3(Mathf.Sin(agent.orientation), 0, Mathf.Cos(agent.orientation));

        RaycastHit hit;
        bool collision = Physics.Raycast(agent.position, linear, out hit, 0.05f);

        if (collision)
        {
            linear = maxAcceleration * ((hit.point + hit.normal) - agent.position);
            
        }

        return angular;
    }

    // Attemps to avoid walls by casting a ray and using the normal
    //Wall Avoidance deals with Collision Detection and Collision Prediction
    // when CD is true then its Collision Detection
    // When it is CP then Collision Prediction
    public Vector3 WallAvoidance(Vector3 linear, bool CD, bool CP)
    {

        // Setup collision detection rayVector
        Vector3 rayVector = agent.velocity;

        //IGNORE SELF BY DOING THIS
        int layerMask = 1 << 9;
        layerMask = ~layerMask;

        RaycastHit hit;
        bool collision;
        if(!CP)
            collision = Physics.Raycast(agent.position, rayVector, out hit, lookAhead*15, layerMask);
        else
            collision = Physics.Raycast(agent.position, rayVector, out hit, lookAhead*100, layerMask);

        if (collision)
        {
            Debug.Log(hit.transform.name);
            Vector3 target_position = hit.point + hit.normal * avoidDistance;
            if (!CD && !CP)
                agent.DrawCircle(target_position, 1);
            else
            {
                
                GameObject collidedBoi = (GameObject)Instantiate(LINE, hit.point , Quaternion.identity);

                Destroy(collidedBoi, 3.0f);
            }
            return (target_position - agent.position) * maxAcceleration;
        }


        return linear;

    }

    public Vector3 ChasePlayer()
    {
        Vector3 predicted_position = Player.GetComponent<Rigidbody>().position;

        // Distance to target
        Vector3 direction = Player.GetComponent<Rigidbody>().position - agent.position;
        float distance = direction.sqrMagnitude;


        // Find current speed
        float speed = agent.velocity.sqrMagnitude;

        // Create a prediction
        float prediction = 0;
        if (speed <= (distance / maxPrediction))
        {
            prediction = maxPrediction;
        }
        else
        {
            prediction = distance / speed;
        }



        // Add the target's current speed to prediction
        predicted_position += Player.GetComponent<Rigidbody>().velocity * prediction;

        agent.DrawCircle(predicted_position, 0.5f);

        // Return vector to predicted location
        return predicted_position - agent.position;
    }
    
    // ETC.

}
