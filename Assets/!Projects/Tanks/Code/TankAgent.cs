using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KartGame.KartSystems;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;

[RequireComponent(typeof(ArcadeKart))]
[RequireComponent(typeof(TankCanonHandle))]
public class TankAgent : Agent, IInput
{
    public Transform SpawnPoint;
    public int MaxHealth = 3;

    private ArcadeKart m_Kart;
    private float m_Acceleration;
    private float m_Steering;
    private TankCanonHandle m_tankCanonHandle;
    private int _currentHealth;

    public override void Initialize()
    {
        m_Kart = GetComponent<ArcadeKart>();
        m_tankCanonHandle = GetComponent<TankCanonHandle>();
        SetResetParameters();
    }

    public override void OnEpisodeBegin()
    {
        //Reset the parameters when the Agent is reset.
        SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(m_tankCanonHandle.CurrentCanonAngle / m_tankCanonHandle.MaxCanonAngle);
        sensor.AddObservation(m_tankCanonHandle.CurrentTowerAngle / 360f);
        sensor.AddObservation(m_tankCanonHandle.CanShootNow());
        sensor.AddObservation(_currentHealth);
        sensor.AddObservation(m_Kart.LocalSpeed());
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "wall")
        {
            SetReward(-1f);
        }
    }

    internal void HandleEnemyHit(TankAgent EnemyHit)
    {
        SetReward(20);
        EnemyHit.SetReward(-20);

        EnemyHit._currentHealth--;
        if(EnemyHit._currentHealth <= 0)
        {
            EnemyHit.SetReward(-100);
            EndEpisode();
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        m_Acceleration = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        m_Steering = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);
        float TowerAngle = Mathf.Clamp(actionBuffers.ContinuousActions[2], 0, 1f);
        float CanonAngle = Mathf.Clamp(actionBuffers.ContinuousActions[3], -1f, 1f);
        m_tankCanonHandle.SetAngle(TowerAngle, CanonAngle);
        if(actionBuffers.ContinuousActions[4] >= 1)
        {
            m_tankCanonHandle.Shoot();
        }
    }

    public void SetResetParameters()
    {
        transform.position = SpawnPoint.position;
        transform.rotation = SpawnPoint.rotation;
        m_Kart.Reset();
        _currentHealth = MaxHealth;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
        continuousActionsOut[2] = 0;
        continuousActionsOut[3] = -0.5f;
        continuousActionsOut[4] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    public Vector2 GenerateInput()
    {
        return new Vector2(m_Steering, m_Acceleration);
    }
}
