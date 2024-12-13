using UnityEngine;
using UnityEngine.AI;

public class VehicleController : MonoBehaviour
{
    public Transform[] waypoints; // Assign waypoints in the Inspector
    private int currentWaypointIndex;
    private NavMeshAgent agent;

    public float vehicleSpeed = 20f; // Adjust speed as needed
    public float stoppingDistance = 2f; // Adjust to avoid collisions with obstacles/other vehicles

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Set NavMeshAgent speed and stopping distance
        agent.speed = vehicleSpeed;
        agent.acceleration = vehicleSpeed * 2; // Adjust for smoother speed transitions
        agent.angularSpeed = 120f; // Higher angular speed for better turning
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true; // Enable smooth stopping

        // Pick the closest waypoint as the starting point
        currentWaypointIndex = FindClosestWaypointIndex();
        MoveToWaypoint();
    }

    void Update()
    {
        // Check if the vehicle has reached the current waypoint
        if (!agent.pathPending && agent.remainingDistance < stoppingDistance)
        {
            MoveToNextWaypoint();
        }
    }

    int FindClosestWaypointIndex()
    {
        int closestIndex = 0;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < waypoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, waypoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    void MoveToWaypoint()
    {
        if (waypoints.Length == 0) return;

        // Calculate a position slightly before the waypoint
        Vector3 directionToWaypoint = (waypoints[currentWaypointIndex].position - transform.position).normalized;
        Vector3 stopPosition = waypoints[currentWaypointIndex].position - directionToWaypoint * stoppingDistance;

        agent.SetDestination(stopPosition);
    }


    void MoveToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        // Increment waypoint index and loop back if necessary
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        MoveToWaypoint();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with a building
        if (collision.gameObject.CompareTag("Buildings"))
        {
            // Turn the vehicle by a certain angle
            TurnVehicle(10f); // Example: turn 90 degrees
        }
    }

    void TurnVehicle(float angle)
    {
        // Rotate the vehicle by the given angle
        transform.Rotate(0, angle, 0);

        // Update the NavMeshAgent destination to move in the new direction
        agent.SetDestination(transform.position + transform.forward * 100f); // Move forward 10 units
    }


}
