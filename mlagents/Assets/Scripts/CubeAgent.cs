using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CubeAgent : Agent
{
    public Transform Target;
    public Transform GoalZone;
    public float speedMultiplier = 0.5f;
    private bool hasTarget = false;

    public override void OnEpisodeBegin()
    {
        hasTarget = false;
        Target.gameObject.SetActive(true);

        // Reset agent
        this.transform.localPosition = new Vector3(0, 0.5f, 0);
        this.transform.localRotation = Quaternion.identity;

        // Verplaats de bol naar een nieuwe plek
        Target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Space Size moet 10 zijn!
        sensor.AddObservation(this.transform.localPosition); // 3
        sensor.AddObservation(GoalZone.localPosition);      // 3
        sensor.AddObservation(hasTarget);                   // 1
        
        if (!hasTarget)
            sensor.AddObservation(Target.localPosition);    // 3
        else
            sensor.AddObservation(this.transform.localPosition); // 3 (dummy)
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // DE BEWEGING (die je vergeten was...)
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        transform.Translate(controlSignal * speedMultiplier);

        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);
        float distanceToGoal = Vector3.Distance(this.transform.localPosition, GoalZone.localPosition);

        // Tijdstraf om luiheid te voorkomen
        AddReward(-0.001f);

        if (!hasTarget)
        {
            if (distanceToTarget < 1.1f)
            {
                hasTarget = true;
                AddReward(1.0f); 
                Target.gameObject.SetActive(false);
            }
        }
        else
        {
            if (distanceToGoal < 1.5f)
            {
                SetReward(2.0f); 
                EndEpisode();
            }
        }

        // Van het platform gevallen
        if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
}