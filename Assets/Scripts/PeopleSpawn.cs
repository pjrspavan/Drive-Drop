using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeopleSpawn : MonoBehaviour
{
    public int AICount = 2;
    public float groundZMin;
    public float minDistanceFromPlayer = 5.0f;
    public float groundZMax;

    public GameObject AIGameObject;
    public GameObject ground;
    private void Start()
    {
        for (int i = 0; i < AICount; i++)
        {
            var AIPosition = GetAIPosition();
            Instantiate(AIGameObject, AIPosition, Quaternion.identity);
        }
    }


    private Vector3 GetAIPosition()
    {
        Bounds groundBounds = ground.GetComponent<Renderer>().bounds;
        Vector3 randomPosition = new Vector3();
        bool isColliding = true;
        while (isColliding)
        {
            randomPosition = new Vector3(
                Random.Range(-99.3f, -99.3f),
                ground.transform.position.y + 0.6f,
                Random.Range(-10f, 10f)
            );

            var colliders = Physics.OverlapBox(randomPosition, new Vector3(1.0f, 0.5f, 1.0f));
            if (colliders.Length == 0)
                isColliding = false;
        }
        return randomPosition;
    }
}