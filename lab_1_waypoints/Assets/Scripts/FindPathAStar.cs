using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem; // Vergeet deze niet als je het nieuwe systeem gebruikt!

// De Shuffle extensie moet BUITEN je klasse staan
/* public static class ListExtensions {
    private static System.Random rng = new System.Random();
    public static void Shuffle<T>(this List<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
} */
public class PathMarker {
    public MapLocation location;
    public float G, H, F;
    public GameObject marker;
    public PathMarker parent;

    public PathMarker(MapLocation l, float g, float h, float f, GameObject m, PathMarker p) {
        location = l;
        G = g;
        H = h;
        F = f;
        marker = m;
        parent = p;
    }

    public override bool Equals(object obj) {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;
        return location.Equals(((PathMarker)obj).location);
    }
}
public class FindPathAStar : MonoBehaviour {
    public Maze maze;
    public Material closedMaterial;
    public Material openMaterial;
    public GameObject startPrefab;
    public GameObject endPrefab;
    public GameObject pathP;
    public GameObject tank; // Sleep je tank/player prefab hierin in de Inspector

    PathMarker startNode;
    PathMarker goalNode;
    PathMarker lastPos;
    bool done = false;

    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();
    List<PathMarker> path = new List<PathMarker>();

    void RemoveAllMarkers() {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach (GameObject m in markers) Destroy(m);
        
        // Gebruik Try-Catch of check op null, anders crasht het als ze er niet zijn!
        Destroy(GameObject.FindGameObjectWithTag("Goal"));
        Destroy(GameObject.FindGameObjectWithTag("Player"));
    }

    void BeginSearch() {
        done = false;
        path.Clear();
        RemoveAllMarkers();

        // Start en Goal posities (Let op de grenzen!)
        startNode = new PathMarker(new MapLocation(1, 1), 0, 0, 0, 
            Instantiate(startPrefab, new Vector3(maze.scale, 0.5f, maze.scale), Quaternion.identity), null);
        
        int gx = Random.Range(2, maze.width - 1);
        int gz = Random.Range(2, maze.depth - 1);
        goalNode = new PathMarker(new MapLocation(gx, gz), 0, 0, 0, 
            Instantiate(endPrefab, new Vector3(gx * maze.scale, 0.5f, gz * maze.scale), Quaternion.identity), null);
        
        goalNode.marker.tag = "Goal"; // Zorg dat de tag klopt!

        open.Clear();
        closed.Clear();
        open.Add(startNode);
        lastPos = startNode;

        StopAllCoroutines();
        StartCoroutine(Searching()); // Laat de computer het werk doen!
    }

    IEnumerator Searching()
    {
    Debug.Log("Zoektocht gestart...");

    // Blijf zoeken zolang we niet klaar zijn en er nog opties in de 'open' lijst staan
    while (!done && open.Count > 0)
    {
        Search(lastPos); // Doe één stap van het algoritme
        
        // DIT IS HET GEHEIM: Wacht een heel klein beetje voor de volgende stap
        // Je kunt 0.05f aanpassen om het sneller of langzamer te maken
        yield return new WaitForSeconds(0.05f); 
    }

    if (done)
    {
        Debug.Log("Pad gevonden! Nu reconstrueren...");
        ReconstructPath();
    }
    else
    {
        Debug.Log("Geen pad mogelijk. Je hebt jezelf opgesloten!");
    }
    }

    void Search(PathMarker thisNode) {
        if (thisNode.location.Equals(goalNode.location)) {
            done = true;
            return;
        }

        foreach (MapLocation dir in maze.directions) {
            MapLocation neighbour = dir + thisNode.location;

            // Verbeterde grenscontrole!
            if (neighbour.x < 0 || neighbour.x >= maze.width || neighbour.z < 0 || neighbour.z >= maze.depth) continue;
            if (maze.map[neighbour.x, neighbour.z] == 1) continue;
            if (IsClosed(neighbour)) continue;

            float g = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
            float h = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
            float f = g + h;

            if (!UpdateMarker(neighbour, g, h, f, thisNode)) {
                GameObject go = Instantiate(pathP, new Vector3(neighbour.x * maze.scale, 0, neighbour.z * maze.scale), Quaternion.identity);
                go.tag = "marker";
                open.Add(new PathMarker(neighbour, g, h, f, go, thisNode));
            }
        }

        open = open.OrderBy(p => p.F).ToList();
        PathMarker pm = open[0];
        closed.Add(pm);
        open.RemoveAt(0);

        pm.marker.GetComponent<Renderer>().material = closedMaterial;
        lastPos = pm;
    }

    void ReconstructPath() {
    PathMarker begin = lastPos;
    List<Vector3> worldPositions = new List<Vector3>();

    while (begin != null) {
        path.Insert(0, begin);
        // We berekenen de wereldpositie van het grid-punt
        worldPositions.Insert(0, new Vector3(begin.location.x * maze.scale, 0, begin.location.z * maze.scale));
        
        begin.marker.GetComponent<Renderer>().material = openMaterial;
        begin = begin.parent;
    }

    // LET OP: We noemen hier de functie die Vector3-lijsten accepteert!
    tank.GetComponent<FollowWaypoint>().StartFollowingPathFromPositions(worldPositions);
    }

    // Helper methodes blijven hetzelfde...
    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt) {
        foreach (PathMarker p in open) {
            if (p.location.Equals(pos)) {
                if (g < p.G) { // Alleen updaten als dit een korter pad is!
                    p.G = g; p.F = f; p.parent = prt;
                    return true;
                }
                return true; 
            }
        }
        return false;
    }

    bool IsClosed(MapLocation marker) {
        return closed.Any(p => p.location.Equals(marker));
    }

    void Update() {
        if (Keyboard.current.pKey.wasPressedThisFrame) BeginSearch();
    }
}