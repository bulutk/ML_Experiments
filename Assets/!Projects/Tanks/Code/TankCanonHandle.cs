using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankCanonHandle : MonoBehaviour
{
    public Transform Tower;
    public Transform Canon;
    public Transform ShootingPoint;

    public MissleHandle MissilePref;
    public float MissileSpeed = 100;

    public float MaxCanonAngle = 25;

    public float TowerRotationPerSec = 30;
    public float CanonRotationPerSec = 20;

    public float ShootCooldownSecs = 2;

    public float CurrentTowerAngle { get; private set; }
    public float CurrentCanonAngle { get; private set; }

    private float _targetTowerAngle;
    private float _targetCanonAngle;
    private float _lastShootTime = 0;
    private TankAgent _ownerTank;

    private void Awake()
    {
        _ownerTank = GetComponent<TankAgent>();
        MissilePref.CreatePool();
    }

    public void SetAngle(float TowerAngle, float CanonAngle)
    {
        _targetTowerAngle = TowerAngle * 360f;
        _targetCanonAngle = Mathf.Clamp(CanonAngle * MaxCanonAngle, -MaxCanonAngle, MaxCanonAngle);
    }

    public void Shoot()
    {
        if (_lastShootTime + ShootCooldownSecs > Time.time)
            return;

        _lastShootTime = Time.time;
        var missle = MissilePref.Spawn(ShootingPoint.position, ShootingPoint.rotation);
        missle.SetOwner(_ownerTank);
        var rigid = missle.GetComponent<Rigidbody>();
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        rigid.AddRelativeForce(new Vector3(0, 0, MissileSpeed));
    }

    public bool CanShootNow()
    {
        return _lastShootTime + ShootCooldownSecs <= Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(CurrentTowerAngle != _targetTowerAngle)
        {
            float diff = _targetTowerAngle - CurrentTowerAngle;
            float adjustedTarget = _targetTowerAngle;
            if (diff > 180)
            {
                adjustedTarget -= 360;
            }
            else if (diff <-180)
            {
                adjustedTarget += 360;
            }

            float adjustedDiff = adjustedTarget - CurrentTowerAngle;
            CurrentTowerAngle += Mathf.Clamp(adjustedDiff, -TowerRotationPerSec, TowerRotationPerSec) * Time.deltaTime;
            CurrentTowerAngle %= 360;

            var rot = Tower.localRotation;
            rot.eulerAngles = new Vector3(0, CurrentTowerAngle, 0);
            Tower.localRotation = rot;
        }

        if (CurrentCanonAngle != _targetCanonAngle)
        {
            float diff = _targetCanonAngle - CurrentCanonAngle;
            CurrentCanonAngle += Mathf.Clamp(diff, -CanonRotationPerSec, CanonRotationPerSec) * Time.deltaTime;

            var rot = Canon.localRotation;
            rot.eulerAngles = new Vector3(CurrentCanonAngle, 0, 0);
            Canon.localRotation = rot;
        }
    }
}
