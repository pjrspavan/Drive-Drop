using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public float speed = 10f;            // Speed of the car
    public float turnSpeed = 50f;        // Turn speed of the car
    public Vector3 dropAreaSize = new Vector3(20f, 0f, 20f); // Size of the drop-off area
    public Transform dropAreaCenter;     // Center of the drop-off area
    public GameObject destinationMarkerPrefab; // Marker to indicate the destination
    public LineRenderer routeLine;      // LineRenderer to display the route
    private GameObject ridePromptUI = null;     // UI prompt for accepting the ride (e.g., "Press Space to accept the ride")

    private Rigidbody rb;
    private bool passengerNearby = false; // Is the car near a passenger?
    private bool rideAccepted = false;    // Has the user accepted the ride?
    private GameObject currentPassenger;  // Reference to the nearby passenger
    private GameObject destinationMarker; // Marker for the destination
    private Vector3 dropLocation;         // Stores the destination location
    private float dropOffRange = 5f;      // Distance from the drop location to consider the car close enough to drop off the passenger
    private Vector3 passengerDropOffset = new Vector3(3f, 0f, 3f); // Offset distance from the car when dropping the passenger

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Ensure the LineRenderer is disabled initially
        if (routeLine != null)
        {
            routeLine.enabled = false;
        }

        // Hide the ride prompt UI initially
        if (ridePromptUI != null)
        {
            ridePromptUI.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        // Get input for movement and turning
        float moveInput = Input.GetAxis("Vertical");   // W/S or Up/Down keys
        float turnInput = Input.GetAxis("Horizontal"); // A/D or Left/Right keys

        // Move the car forward/backward
        Vector3 movement = transform.forward * moveInput * speed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);

        // Rotate the car
        float turn = turnInput * turnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);

        // Accept the ride and pick up the passenger when space bar is pressed
        if (passengerNearby && !rideAccepted && Input.GetKeyDown(KeyCode.Space))
        {
            AcceptRide();
        }

        // Update the route if the ride is accepted
        if (rideAccepted && routeLine != null)
        {
            UpdateRouteLine(transform.position, dropLocation);
        }

        // Check if the car is close enough to the destination to drop off the passenger
        if (rideAccepted && Vector3.Distance(transform.position, dropLocation) < dropOffRange)
        {
            // Prompt the user to drop off the passenger when space is pressed
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DropOffPassenger();
            }
        }
    }

    // Trigger-based passenger detection (instead of collision detection)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Passenger") && !rideAccepted)
        {
            currentPassenger = other.gameObject; // Store the reference to the passenger
            passengerNearby = true;

            // Show the UI prompt to accept the ride
            if (ridePromptUI != null)
            {
                ridePromptUI.SetActive(true);
            }
        }
    }

    // Trigger-based exit detection for passengers
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Passenger") && !rideAccepted)
        {
            passengerNearby = false;

            // Hide the UI prompt if the passenger moves out of range
            if (ridePromptUI != null)
            {
                ridePromptUI.SetActive(false);
            }
        }
    }

    // Method to accept the ride
    private void AcceptRide()
    {
        Debug.Log("Ride accepted! Picking up passenger...");
        rideAccepted = true;
        passengerNearby = false;

        // Disable the passenger object to simulate picking them up
        if (currentPassenger != null)
            currentPassenger.SetActive(false);

        // Generate a random drop-off location on the NavMesh
        dropLocation = GenerateRandomDropLocation();
        Debug.Log($"Passenger drop-off location: {dropLocation}");

        // Place a destination marker at the drop-off location
        if (destinationMarkerPrefab != null)
        {
            if (destinationMarker != null)
                Destroy(destinationMarker); // Remove the old marker, if any

            destinationMarker = Instantiate(destinationMarkerPrefab, dropLocation, Quaternion.identity);
        }

        // Enable the route line
        if (routeLine != null)
        {
            routeLine.enabled = true;
        }

        // Hide the ride prompt UI after the ride is accepted
        if (ridePromptUI != null)
        {
            ridePromptUI.SetActive(false);
        }
    }

    // Drop off the passenger at the drop location
    private void DropOffPassenger()
    {
        Debug.Log("Dropping off the passenger...");

        // Instantiate the passenger at a position near the drop location
        if (currentPassenger != null)
        {
            // Add an offset to ensure the passenger is placed at some distance from the car
            Vector3 dropPosition = dropLocation + passengerDropOffset;

            // Re-enable the passenger and place them at the drop location
            currentPassenger.transform.position = dropPosition;
            currentPassenger.SetActive(true);  // Make sure the passenger is active again after being deactivated

            // Clear the reference to the current passenger
            currentPassenger = null;
        }

        // Reset the car state and disable route line
        rideAccepted = false;
        if (routeLine != null)
        {
            routeLine.enabled = false;
        }

        // Reset the destination marker
        if (destinationMarker != null)
        {
            Destroy(destinationMarker);
        }
    }

    // Generate a random drop-off location ensuring it's on the NavMesh
    private Vector3 GenerateRandomDropLocation()
    {
        // Generate a random position within the defined drop area
        Vector3 randomPosition = new Vector3(
            Random.Range(-dropAreaSize.x / 2, dropAreaSize.x / 2),
            0f, // Assuming the area is flat on the ground
            Random.Range(-dropAreaSize.z / 2, dropAreaSize.z / 2)
        );

        // Offset the random position to align with the drop area center
        Vector3 candidatePosition = dropAreaCenter.position + randomPosition;

        // Use NavMesh.SamplePosition to ensure the position is on a NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidatePosition, out hit, 10f, NavMesh.AllAreas))
        {
            // Return the closest valid position on the NavMesh
            return hit.position;
        }
        else
        {
            Debug.LogWarning("No valid NavMesh position found. Retrying...");
            // Retry generating a valid position (you could loop this or return a default position)
            return GenerateRandomDropLocation();
        }
    }

    // Update the route line between the start and end positions
    private void UpdateRouteLine(Vector3 startPosition, Vector3 endPosition)
    {
        // Adjust the y-position of the start and end points
        Vector3 startOffset = new Vector3(startPosition.x, startPosition.y + 1f, startPosition.z);
        Vector3 endOffset = new Vector3(endPosition.x, endPosition.y + 1f, endPosition.z);

        // Update the LineRenderer positions
        routeLine.SetPosition(0, startOffset); // Start position with y-offset
        routeLine.SetPosition(1, endOffset);   // End position with y-offset
    }

    // Draw the drop area and test random drop location in the Scene view for visualization
    private void OnDrawGizmosSelected()
    {
        if (dropAreaCenter != null)
        {
            // Draw the drop area in the Scene view for visualization
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(dropAreaCenter.position, dropAreaSize);

            // Debugging: Visualize the random position with a red sphere
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(dropAreaCenter.position + GenerateRandomDropLocation(), 0.5f);
        }
    }
}
