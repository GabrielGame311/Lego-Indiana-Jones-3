using UnityEngine;

public class AutoTrain : MonoBehaviour
{
    public Transform[] waypoints; // Dra in alla punkter här
    public float speed = 10f;
    public float rotationSpeed = 5f;
    
    private int currentWaypointIndex = 0;

    void Update()
    {
        if (waypoints.Length == 0) return;

        // 1. Hitta mål-punkten
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // 2. Beräkna riktning mot punkten
        Vector3 direction = targetWaypoint.position - transform.position;
        direction.y = 0; // Håll tåget plant på rälsen

        // 3. Rota tåget mjukt mot nästa punkt
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 4. Flytta tåget framåt
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

        // 5. När tåget är framme vid punkten, gå till nästa
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.2f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length; // Loopar runt banan
        }
    }


    
}