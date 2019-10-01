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

    public float mapToRange(float rotation)
    {
        float rotationRad = rotation * Mathf.Deg2Rad;
        if (rotationRad <= (-1 * Mathf.PI))
        {
            rotationRad += 2 * Mathf.PI; //RotationRad is negative and we want it to be positive
        }
        else if (rotationRad > Mathf.PI)
        {
            rotationRad += (-2 * Mathf.PI);
        }

        rotationRad = rotationRad - 2 * Mathf.PI * Mathf.Floor((rotationRad + Mathf.PI) / (2 * Mathf.PI));
        return rotationRad;
    }

    public Vector3 Seek()
    {
        return target.position - agent.position;
    }

    public Vector3 Flee()
    {
        return agent.position - target.position;
    }

    public Vector3 Arrive()
    {
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

        agent.DrawCircle(predicted_position, 0.5f);

        // Return vector away from predicted location
        return agent.position - predicted_position;
    }

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

        // Map the result to the (0, 2pi) interval
        while (rotation > Mathf.PI)
        {
            rotation -= 2 * Mathf.PI;
        }
        while (rotation < -Mathf.PI)
        {
            rotation += 2 * Mathf.PI;
        }
        float rotationSize = Mathf.Abs(rotation);

        // Check if we are there, return no steering
        if (rotationSize < targetRadiusA)
        {
            agent.rotation = 0;
        }

        // If we are outside the slowRadius, then use max rotation
        // Otherwise calculate a scaled rotation
        float targetRotation = (rotationSize > slowRadiusA ? maxRotation : maxRotation * rotationSize / slowRadiusA);

        // The final target rotation combines speed (already in the variable) and direction
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

        // Map the result to the (0, 2pi) interval
        while (rotation > Mathf.PI)
        {
            rotation -= 2 * Mathf.PI;
        }
        while (rotation < -Mathf.PI)
        {
            rotation += 2 * Mathf.PI;
        }
        float rotationSize = Mathf.Abs(rotation);

        // Check if we are there, return no steering
        if (rotationSize < targetRadiusA)
        {
            agent.rotation = 0;
        }

        // If we are outside the slowRadius, then use max rotation
        // Otherwise calculate a scaled rotation
        float targetRotation = (rotationSize > slowRadiusA ? maxRotation : maxRotation * rotationSize / slowRadiusA);

        // The final target rotation combines speed (already in the variable) and direction
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

    public float Align()
    {
        float rotation = Vector3.Angle(Vector3.forward, target.position) - agent.transform.eulerAngles.y;
        print(Vector3.Angle(Vector3.forward, target.position));

        rotation = mapToRange(rotation);
        float rotationSize = Mathf.Abs(rotation);

        float targetRotation = maxRotation;
        targetRotation *= rotation / rotationSize;

        return targetRotation - agent.rotation;

    }

    public float Wander(out Vector3 linear)
    {
        // Update the wander orientation
        wanderOrientation += (Random.value - Random.value) * wanderRate;

        // Calculate the combined target orientation
        float orientation = wanderOrientation + agent.orientation;

        // Calculate the center of the wander circle
        Vector3 position = agent.position + wanderOffset * new Vector3(Mathf.Sin(agent.orientation), 0, Mathf.Cos(agent.orientation));
        

        // Calculate the target location
        position += wanderRadius * new Vector3(Mathf.Sin(orientation), 0, Mathf.Cos(orientation));

        // Work out the direction to target
        Vector3 direction = position - agent.position;

        // Check for a zero direction, and make no change if so
        if (direction.magnitude == 0)
        {
            linear = Vector2.zero;
            return 0;
        }

        // Get the naive direction to the target
        float rotation = Mathf.Atan2(direction.x, direction.z) - agent.orientation;

        // Map the result to the (0, 2pi) interval
        while (rotation > Mathf.PI)
        {
            rotation -= 2 * Mathf.PI;
        }
        while (rotation < -Mathf.PI)
        {
            rotation += 2 * Mathf.PI;
        }
        float rotationSize = Mathf.Abs(rotation);

        // Check if we are there, return no steering
        if (rotationSize < targetRadiusA)
        {
            agent.rotation = 0;
        }

        // If we are outside the slowRadius, then use max rotation
        // Otherwise calculate a scaled rotation
        float targetRotation = (rotationSize > slowRadiusA ? maxRotation : maxRotation * rotationSize / slowRadiusA);

        // The final target rotation combines speed (already in the variable) and direction
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

    public Vector3 WallAvoidance(Vector3 linear, bool CD, bool CP)
    {

        // Setup collision detection rayVector
        Vector3 rayVector = agent.velocity;


        RaycastHit hit;
        bool collision;
        if(!CP)
            collision = Physics.Raycast(agent.position, rayVector, out hit, lookAhead);
        else
            collision = Physics.Raycast(agent.position, rayVector, out hit, lookAhead*3);

        if (collision)
        {
            Debug.Log(hit.distance);
            Vector3 target_position = hit.point + hit.normal * avoidDistance;
            if (!CD && !CP)
                agent.DrawCircle(target_position, 1);
            else
            {
                
                GameObject collidedBoi = (GameObject)Instantiate(LINE, agent.position - target_position , Quaternion.identity);

                Destroy(collidedBoi, 3.0f);
            }
            return (target_position - agent.position) * maxAcceleration;
        }


        return linear;

    }

    public Vector3 CollisionDetection (Vector3 linear)
    {
        collided = true;
        // Setup collision detection rayVector
        Vector3 rayVector = agent.velocity;


        RaycastHit hit;
        bool collision = Physics.Raycast(agent.position, rayVector, out hit, lookAhead);

        if (collision)
        {
            Debug.Log(hit.distance);
            Vector3 target_position = hit.point + hit.normal * avoidDistance;
            GameObject.Find("Line").GetComponent<liner>().DrawCircle(target_position, 1);
            return (target_position - agent.position) * maxAcceleration;
        }


        return linear;

    }

    public Vector3 CollisionPrediction(Vector3 linear)
    {

        // Setup collision detection rayVector
        Vector3 rayVector = agent.velocity;


        RaycastHit hit;
        bool collision = Physics.Raycast(agent.position, rayVector, out hit, lookAhead);

        if (collision)
        {
            Debug.Log(hit.distance);
            Vector3 target_position = hit.point + hit.normal * avoidDistance;
            agent.DrawCircle(target_position, 1);
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
