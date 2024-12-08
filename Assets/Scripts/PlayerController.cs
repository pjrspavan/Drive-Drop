using UnityEngine;
public class PlayerController : MonoBehaviour
{
    public float speed = 10f;
    public float turnSpeed = 50f;
    public GameObject destinationMarkerPrefab; // Marker to indicate the destination
    public LineRenderer routeLine;            // LineRenderer to display the route
    public GameObject ridePromptUI;           // UI prompt for accepting the ride

    public float minX = -50f; // Minimum x-coordinate for drop-off area
    public float maxX = 50f;  // Maximum x-coordinate for drop-off area
    public float minZ = -50f; // Minimum z-coordinate for drop-off area
    public float maxZ = 50f;  // Maximum z-coordinate for drop-off area

    private Rigidbody rb;
    private bool passengerNearby = false;
    private bool rideAccepted = false;
    private GameObject currentPassenger;
    private GameObject destinationMarker;
    private Vector3 dropLocation;
    private float dropOffRange = 5f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (routeLine != null)
            routeLine.enabled = false;

        if (ridePromptUI != null)
            ridePromptUI.SetActive(false);
    }

    private void FixedUpdate()
    {
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        Vector3 movement = transform.forward * moveInput * speed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);

        float turn = turnInput * turnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);

        if (passengerNearby && !rideAccepted && Input.GetKeyDown(KeyCode.Space))
        {
            AcceptRide();
        }

        if (rideAccepted && routeLine != null)
        {
            UpdateRouteLine(transform.position, dropLocation);
        }

        if (rideAccepted && Vector3.Distance(transform.position, dropLocation) < dropOffRange)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DropOffPassenger();
            }
        }
    }

    private void AcceptRide()
    {
        Debug.Log("Ride accepted! Picking up passenger...");
        rideAccepted = true;
        passengerNearby = false;

        if (currentPassenger != null)
            currentPassenger.SetActive(false);

        dropLocation = GenerateRandomDropLocation();
        Debug.Log($"Drop-off location: {dropLocation}");

        if (destinationMarkerPrefab != null)
        {
            if (destinationMarker != null)
                Destroy(destinationMarker);

            destinationMarker = Instantiate(destinationMarkerPrefab, dropLocation, Quaternion.identity);
        }

        // Update the route line based on the NavMesh path
        if (routeLine != null)
        {
            routeLine.enabled = true;
            UpdateRouteLine(transform.position, dropLocation);
        }

        if (ridePromptUI != null)
            ridePromptUI.SetActive(false);
    }

    private void UpdateRouteLine(Vector3 startPosition, Vector3 endPosition)
    {
        // Calculate a NavMesh path
        UnityEngine.AI.NavMeshPath navMeshPath = new UnityEngine.AI.NavMeshPath();
        if (UnityEngine.AI.NavMesh.CalculatePath(startPosition, endPosition, UnityEngine.AI.NavMesh.AllAreas, navMeshPath))
        {
            // Set the number of points in the LineRenderer
            routeLine.positionCount = navMeshPath.corners.Length;

            // Update the LineRenderer with the path points
            for (int i = 0; i < navMeshPath.corners.Length; i++)
            {
                Vector3 point = navMeshPath.corners[i];
                point.y += 0.5f; // Slightly raise the points above the road for better visibility
                routeLine.SetPosition(i, point);
            }
        }
        else
        {
            Debug.LogWarning("Could not calculate a NavMesh path.");
            routeLine.positionCount = 0; // Clear the LineRenderer if no path is found
        }
    }


    private void DropOffPassenger()
    {
        Debug.Log("Dropping off the passenger...");

        if (currentPassenger != null)
        {
            Vector3 dropPosition = dropLocation + new Vector3(3f, 0f, 3f);
            currentPassenger.transform.position = dropPosition;
            currentPassenger.SetActive(true);
            currentPassenger = null;
        }

        rideAccepted = false;

        if (routeLine != null)
            routeLine.enabled = false;

        if (destinationMarker != null)
            Destroy(destinationMarker);
    }

    private Vector3 GenerateRandomDropLocation()
    {
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);
        Vector3 candidatePosition = new Vector3(randomX, 0f, randomZ);

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(candidatePosition, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }

        Debug.LogWarning("No valid NavMesh position found. Retrying...");
        return GenerateRandomDropLocation();
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Passenger") && !rideAccepted)
        {
            currentPassenger = other.gameObject;
            passengerNearby = true;

            if (ridePromptUI != null)
                ridePromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Passenger") && !rideAccepted)
        {
            passengerNearby = false;

            if (ridePromptUI != null)
                ridePromptUI.SetActive(false);
        }
    }
}
