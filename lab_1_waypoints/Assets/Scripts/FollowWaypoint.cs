using UnityEngine;
using System.Collections.Generic;

public class FollowWaypoint : MonoBehaviour {
    // SLECHTS ÉÉN LIST! Geen dubbele namen meer!
    private List<Vector3> waypoints = new List<Vector3>();
    private int currentWP = 0;
    private bool isMoving = false;

    public float speed = 5.0f;
    public float rotSpeed = 5.0f;
    public float stopDistance = 1.0f; // Iets ruimer voor soepeler rijden

    // Deze functie koppelt perfect met je NavigationController
    public void StartFollowingPathFromPositions(List<Vector3> worldPath) {
    waypoints = worldPath; // Geen omzetting nodig, het zijn al Vector3's!
    currentWP = 0;
    isMoving = true;
    Debug.Log("Tank volgt grid-pad van " + waypoints.Count + " punten.");
    }
    
    public void StartFollowingPath(List<GameObject> path) {
    waypoints.Clear();
    foreach (GameObject go in path) {
        waypoints.Add(go.transform.position);
    }
    currentWP = 0;
    isMoving = true;
    }

    void Update() {
        // Stop als we niet bewegen of geen pad hebben
        if (!isMoving || waypoints.Count == 0) return;

        // 1. Richting bepalen naar het huidige waypoint
        Vector3 targetPos = waypoints[currentWP];
        // Zorg dat de tank op dezelfde hoogte blijft als het doel
        targetPos.y = transform.position.y; 
        
        Vector3 direction = targetPos - transform.position;

        if (direction.magnitude > stopDistance) {
            // 2. Draaien naar het volgende punt
            if (direction != Vector3.zero) {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotSpeed);
            }

            // 3. Vooruit rijden (Lokaal Z-as)
            transform.Translate(0, 0, speed * Time.deltaTime);
        } else {
            // We zijn dichtbij genoeg! Op naar de volgende palmboom...
            currentWP++;
            
            // Check of we bij de allerlaatste palmboom zijn
            if (currentWP >= waypoints.Count) {
                isMoving = false;
                Debug.Log("Bestemming bereikt! De tank rust uit onder de palmboom.");
            }
        }
    }
}