using UnityEngine;
using TMPro;
public class PlayerController : MonoBehaviour
{

    public float speed = 10f;
    public float turnSpeed = 50f;
    public GameObject destinationMarkerPrefab; // Marker to indicate the destination
    public LineRenderer routeLine;            // LineRenderer to display the route
    public GameObject ridePromptUI;           // UI prompt for accepting the ride

    public int rides;
    public TMP_Text timerTxt;
    public TMP_Text ridesTxt;
    public float timeRemaining = 300;
    private bool timerRunning = false;
    public float minX = -50f; // Minimum x-coordinate for drop-off area
    public float maxX = 50f;  // Maximum x-coordinate for drop-off area
    public float minZ = -50f; // Minimum z-coordinate for drop-off area
    public float maxZ = 50f;  // Maximum z-coordinate for drop-off area

    private Rigidbody rb;
    private bool passengerNearby = false;
    private bool rideAccepted = false;
    private GameObject currentPassenger;
    private GameObject previousPassenger;
    private GameObject destinationMarker;
    private Vector3 dropLocation;
    private float dropOffRange = 5f;
    public string nextLevel;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        timerRunning = true;
        ridesTxt.text = string.Format("Rides: {0}", rides);

        if (routeLine != null)
            routeLine.enabled = false;

        if (ridePromptUI != null)
            ridePromptUI.SetActive(false);
    }

    private void Update()
    {
        if (timerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay(timeRemaining);
                if (rides == 0)
                    UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevel);
            }
            else
            {
                timeRemaining = 0;
                timerRunning = false;
                if (rides > 0)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Lose Scene");
                }
            }
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        // Convert seconds to minutes and seconds
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        // Update the UI text
        timerTxt.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
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
        rideAccepted = true;

        passengerNearby = false;

        if (currentPassenger != null)
        {
            if (previousPassenger != null)
            {
                previousPassenger.transform.position = currentPassenger.transform.position;
                previousPassenger.tag = "Passenger";
            }
            currentPassenger.SetActive(false);
        }

        dropLocation = GenerateRandomDropLocation();

        if (destinationMarkerPrefab != null)
        {
            if (destinationMarker != null)
                Destroy(destinationMarker);

            destinationMarker = Instantiate(destinationMarkerPrefab, dropLocation, Quaternion.identity);
        }

        // Update the route line based on the NavMesh path
        if (routeLine != null)
        {
            // Assign the LineRenderer to the RouteLine layer
            routeLine.gameObject.layer = LayerMask.NameToLayer("RouteLine");

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
            currentPassenger.tag = "Untagged";
            previousPassenger = currentPassenger;
            currentPassenger = null;

        }

        rideAccepted = false;
        rides -= 1;
        ridesTxt.text = string.Format("Rides: {0}", rides);

        if (routeLine != null)
            routeLine.enabled = false;

        if (destinationMarker != null)
            Destroy(destinationMarker);
    }

    private Vector3 GenerateRandomDropLocation()
    {
        float minDistanceFromPlayer = 30f; // Minimum distance the drop-off point should be from the player
        Vector3 candidatePosition;
        float playerX = transform.position.x; // Player's current X position
        float playerZ = transform.position.z; // Player's current Z position

        do
        {
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            candidatePosition = new Vector3(randomX, 0f, randomZ);

            // Check if the candidatePosition is far enough from the player
        } while (Vector3.Distance(candidatePosition, new Vector3(playerX, 0f, playerZ)) < minDistanceFromPlayer);

        // Ensure the drop-off location is on the NavMesh
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(candidatePosition, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }

        // If NavMesh fails, retry
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
