using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissleHandle : MonoBehaviour
{
    private TankAgent _ownerAgent;

    public void SetOwner(TankAgent OwnerAgent)
    {
        _ownerAgent = OwnerAgent;
    }

    private void OnEnable()
    {
        StartCoroutine(WaitAndKill());
    }

    IEnumerator WaitAndKill()
    {
        yield return new WaitForSeconds(3);
        this.Recycle();
    }

    private void OnCollisionEnter(Collision collision)
    {
        _ownerAgent.HandleMissileHit(collision);
        this.Recycle();
    }
}
