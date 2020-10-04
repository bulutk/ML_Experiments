using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class TankMap : MonoBehaviour
{
    public RewardBoxHandle RewardBoxPref;

    private RewardBoxHandle _rewardBoxInstance;

    // Start is called before the first frame update
    void Start()
    {
        RewardBoxPref.CreatePool();
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
    }

    private void EnvironmentReset()
    {
        if(_rewardBoxInstance == null)
        {
            _rewardBoxInstance = RewardBoxPref.Spawn(transform.position);
        }
    }
}
