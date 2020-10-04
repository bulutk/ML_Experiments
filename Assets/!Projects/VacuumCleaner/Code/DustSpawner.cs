using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject _dustGameObject;

    [SerializeField]
    private int _dustCount = 10;

    private List<GameObject> _dustInstances;

    public void Initialize()
    {
        _dustInstances = new List<GameObject>(_dustCount);
    }

    public void GenerateDust()
    {
        RemoveDust();

        for (int i = 0; i < _dustCount; i++)
        {
            var randomOffset = new Vector3(UnityEngine.Random.Range(-8f, 8f), 0.15f, UnityEngine.Random.Range(-8f, 8f));
            var go = GameObject.Instantiate(_dustGameObject, transform.position + randomOffset, Quaternion.identity);
            _dustInstances.Add(go);
        }
    }

    public float GetDustLeftPercentage()
    {
        return _dustInstances.Count / _dustCount;
    }

    public void RemoveDust()
    {
        for (int i = _dustInstances.Count - 1; i >= 0; i--)
        {
            if (_dustInstances[i] != null)
                GameObject.Destroy(_dustInstances[i]);
        }
        _dustInstances.Clear();
    }

    //Returns true if this was the last dust
    internal bool RemoveDust(GameObject dustInstance)
    {
        if (dustInstance == null)
            return false;

        _dustInstances.Remove(dustInstance);
        GameObject.Destroy(dustInstance);

        return _dustInstances.Count == 0;
    }
}
