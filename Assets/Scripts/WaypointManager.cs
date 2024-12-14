using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public Transform[] waypoints;

    public Transform GetRandomWaypoint()
    {
        int randomIndex = Random.Range(0, waypoints.Length);
        return waypoints[randomIndex];
    }
}
