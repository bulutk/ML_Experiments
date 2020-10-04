using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KartGame.KartSystems;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(ArcadeKart))]
public class TankAgent : Agent, IInput
{
    public Transform SpawnPoint;

    private ArcadeKart m_Kart;
    private float m_Acceleration;
    private float m_Steering;
    

    public override void Initialize()
    {
        m_Kart = GetComponent<ArcadeKart>();
        SetResetParameters();
    }

    public override void OnEpisodeBegin()
    {
        //Reset the parameters when the Agent is reset.
        SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(1);
        //sensor.AddObservation(0);        
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "wall")
        {
            //Hit wall reset
            SetReward(-20f);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        m_Acceleration = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        m_Steering = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        
        AddReward(m_Kart.LocalSpeed());
    }

    public void SetResetParameters()
    {
        transform.position = SpawnPoint.position;
        transform.rotation = SpawnPoint.rotation;
        m_Kart.Reset();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

    public Vector2 GenerateInput()
        {
            return new Vector2(m_Steering, m_Acceleration);
        }
}
