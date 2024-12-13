using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class VehicleInteraction : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Vehicle"))
        {
            // Push the player away from the vehicle
            Vector3 pushDirection = collision.gameObject.transform.position - transform.position;
            pushDirection.Normalize();
            collision.gameObject.GetComponent<Rigidbody>().AddForce(pushDirection * 0.5f, ForceMode.Impulse);
        }
    }
}

