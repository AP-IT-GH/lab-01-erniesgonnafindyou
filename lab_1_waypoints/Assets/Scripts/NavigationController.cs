using UnityEngine;
using UnityEngine.UI; // Vergeet deze niet!
using System.Collections.Generic;
using TMPro;

public class NavigationController : MonoBehaviour {
    public WPManager wpManager;
    public GameObject tank;
    public TMP_Dropdown targetDropdown;
    
    // We moeten weten waar de tank nu is
    private GameObject currentTankLocation;

    void Start() {
        // Vul de dropdown met namen van palmbomen
        targetDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (GameObject wp in wpManager.waypoints) {
            options.Add(wp.name);
        }
        targetDropdown.AddOptions(options);

        // Stel de startlocatie van de tank in op het dichtstbijzijnde waypoint
        currentTankLocation = FindClosestWaypoint();
    }

    GameObject FindClosestWaypoint() {
        GameObject closest = null;
        float dist = Mathf.Infinity;
        foreach (GameObject wp in wpManager.waypoints) {
            float d = Vector3.Distance(tank.transform.position, wp.transform.position);
            if (d < dist) {
                dist = d;
                closest = wp;
            }
        }
        return closest;
    }

    public void OnGoButtonClick() {
        GameObject targetWP = wpManager.waypoints[targetDropdown.value];
        
        // Roep AStar aan op de graph van wpManager
        if (wpManager.graph.AStar(currentTankLocation, targetWP)) {
            // Haal de lijst met GameObjects uit de graph
            List<GameObject> path = new List<GameObject>();
            for(int i = 0; i < wpManager.graph.pathList.Count; i++) {
                path.Add(wpManager.graph.pathList[i].getID());
            }

            // Geef het pad aan de tank
            tank.GetComponent<FollowWaypoint>().StartFollowingPath(path);
            
            // Update de huidige locatie voor de volgende zoektocht
            currentTankLocation = targetWP;
        }
    }
}