using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class ObelixAgent : Agent
{
    private bool hasMenhir = false;
    public GameObject[] menhirObjects; // Sleep hier je menhir-instanties in
    public GameObject[] destinationObjects; // Sleep hier je bestemmingen in
    public float circleRadius = 3f; // Hoe groot moet de cirkel zijn?
    private float lastDistance;
    
    // Deze methode wordt aangeroepen aan het begin van elke nieuwe poging (episode)
    public override void OnEpisodeBegin()
    {
        hasMenhir = false;

        // 1. Obelix resetten naar het midden
        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        transform.localRotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);

        // 2. Bestemmingen in een cirkel plaatsen
        for (int i = 0; i < destinationObjects.Length; i++)
        {
            // Bereken de hoek voor elk object in de cirkel
            float angle = i * Mathf.PI * 2f / destinationObjects.Length;
            Vector3 newPos = new Vector3(Mathf.Cos(angle) * circleRadius, 0.5f, Mathf.Sin(angle) * circleRadius);
            
            destinationObjects[i].transform.localPosition = newPos;
            destinationObjects[i].SetActive(true);
        }

        // 3. Menhirs op willekeurige plekken (binnen het veld van schaal 2, dus -8 tot 8 ongeveer)
        foreach (GameObject m in menhirObjects)
        {
            m.SetActive(true);
            float randomX = Random.Range(-7f, 7f);
            float randomZ = Random.Range(-7f, 7f);
            m.transform.localPosition = new Vector3(randomX, 2f, randomZ); // Ze vallen uit de lucht
        }
    }

    // Hier geef je extra informatie aan het brein die de Ray Sensor niet ziet
    public override void CollectObservations(VectorSensor sensor)
    {
        // Vertel de AI of hij een menhir draagt (1 of 0)
        sensor.AddObservation(hasMenhir ? 1.0f : 0.0f);
    }

    // Hier worden de beslissingen van de AI omgezet in beweging
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveStep = actions.ContinuousActions[0]; // Vooruit/Achteruit
        float rotateStep = actions.ContinuousActions[1]; // Draaien

        transform.Translate(Vector3.forward * moveStep * Time.deltaTime * 20f);
        transform.Rotate(Vector3.up * rotateStep * Time.deltaTime * 150f);

        // Reward Shaping: Beloon hem als hij dichterbij komt
        float distanceToTarget = GetDistanceToCurrentTarget();
        if (distanceToTarget < lastDistance)
        {
            AddReward(0.001f); // Een klein duwtje in de rug
        }
        else
        {
            AddReward(-0.001f); // Straf voor wegwandelen
        }
        lastDistance = distanceToTarget;

    // Bestaande checks (vallen, tijdstraf)

        if (transform.localPosition.y < -1.0f) // Iets onder het platform voor de zekerheid
        {
            Debug.Log("Plons! Obelix dacht dat hij kon zwemmen.");
            AddReward(-5.0f); 
            EndEpisode();
        }

        // De "straf" voor het verstrijken van de tijd (luiheid voorkomen)
        AddReward(-0.002f);
    }

    private float GetDistanceToCurrentTarget()
    {
        if (!hasMenhir)
        {
            // Zoek de dichtstbijzijnde actieve menhir
            return GetClosestActiveTransform(menhirObjects);
        }
        else
        {
            // Zoek de dichtstbijzijnde actieve bestemming
            return GetClosestActiveTransform(destinationObjects);
        }
    }

    private float GetClosestActiveTransform(GameObject[] targets)
    {
        float closestDist = float.MaxValue;
        foreach (var t in targets)
        {
            if (t.activeSelf)
            {
                float d = Vector3.Distance(transform.localPosition, t.transform.localPosition);
                if (d < closestDist) closestDist = d;
            }
        }
        return closestDist;
    }

    // De logica voor botsingen (Beloningen!)
    // Voor de harde botsingen (Menhir)
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Harde botsing met: " + collision.gameObject.name);
        
        if (collision.gameObject.CompareTag("Menhir"))
        {
            HandleMenhirLogic(collision.gameObject);
        }
    }

    // Voor de zones waar je doorheen rijdt (Destination)
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Destination"))
        {
            if (hasMenhir)
            {
                hasMenhir = false; // Hij is hem nu kwijt
                AddReward(5.0f);   // Beloning voor deze drop
                
                other.gameObject.SetActive(false); // Bestemming verdwijnt
                Debug.Log("Menhir afgeleverd! Op naar de volgende...");

                // Check of alles nu opgeruimd is
                if (CheckIfAllDone())
                {
                    Debug.Log("Alles is weg! Obelix heeft rust verdiend.");
                    EndEpisode();
                }
            }
            else 
            {
                Debug.Log("Je bent bij de bestemming, maar je hebt niks bij je!");
            }
        }
    }

    // Een hulpfunctie om te kijken of alle bestemmingen of menhirs op 'inactive' staan
    private bool CheckIfAllDone()
    {
        foreach (GameObject dest in destinationObjects)
        {
            if (dest.activeSelf) return false; // Er is nog minstens één bestemming over
        }
        return true; // Alles is gedeactiveerd
    }

    // Hulpmethode om dubbele code te voorkomen
    private void HandleMenhirLogic(GameObject menhir)
    {
        if (!hasMenhir)
        {
            hasMenhir = true;
            AddReward(2.0f);
            menhir.SetActive(false);
            Debug.Log("Menhir opgepakt!");
        }
    }

    void FixedUpdate()
    {
        foreach (GameObject m in menhirObjects)
        {
            // Als een menhir nog actief is, maar onder het platform valt
            if (m.activeSelf && m.transform.localPosition.y < -1.0f)
            {
                Debug.Log("Menhir verloren! Episode wordt beëindigd.");
                
                // Geef een zware straf, net als bij het zelf vallen
                AddReward(-1.0f); 
                
                // Stop de boel en reset alles
                EndEpisode();
                break; // Stop de loop, we gaan toch resetten
            }
        }
    }

    // Voor handmatige besturing met je toetsenbord (om te testen)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
}