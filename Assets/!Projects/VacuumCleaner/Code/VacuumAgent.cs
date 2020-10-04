using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class VacuumAgent : Agent
{
    [Header("Specific to VacuumCleaner")]
    [SerializeField]
    private float _MaxSpeed = 0.5f;
    [SerializeField]
    private float _MaxBackSpeed = 0.2f;
    [SerializeField]
    private float _acceleration = 0.1f;
    [SerializeField]
    private float _maxRotation = 5;
    [SerializeField]
    private float _detectionAngle = 30;
    [SerializeField]
    private string _wallTag = "wall";
    [SerializeField]
    private string _dustTag = "dust";

    EnvironmentParameters m_ResetParams;
    DustSpawner _dustSpawner;
    private Vector3 _lastPos;
    private Rigidbody _rigidbody;

    private float _currentSpeed = 0;
    private float _rewardAddUp = 0;
    private float _startTime;

    public override void Initialize()
    {
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        _rigidbody = GetComponent<Rigidbody>();
        _dustSpawner = GetComponentInParent<DustSpawner>();
        _dustSpawner.Initialize();
        SetResetParameters();
    }

    public override void OnEpisodeBegin()
    {
        //Reset the parameters when the Agent is reset.
        SetResetParameters();
    }

    public float LidarRotateSpeed = 10;
    float _lastLidarAngle = 0;
    public override void CollectObservations(VectorSensor sensor)
    {
         var forward = transform.forward;
        forward.y = 0;
        sensor.AddObservation(_currentSpeed / _MaxSpeed);
        sensor.AddObservation(_dustSpawner.GetDustLeftPercentage());
        AddRaycastTest(-forward, sensor);

        /*AddRaycastTest(forward, sensor);
        AddRaycastTest(Quaternion.Euler(0, -_detectionAngle, 0) * forward, sensor);
        AddRaycastTest(Quaternion.Euler(0, _detectionAngle, 0) * forward, sensor);

        AddRaycastTest(-forward, sensor);
        AddRaycastTest(Quaternion.Euler(0, -_detectionAngle, 0) * -forward, sensor);
        AddRaycastTest(Quaternion.Euler(0, _detectionAngle, 0) * -forward, sensor);
        */

        /*
        _lastLidarAngle += LidarRotateSpeed;
        _lastLidarAngle %= 360;
        AddRaycastTest(Quaternion.Euler(0, _lastLidarAngle, 0) * forward, sensor);
        sensor.AddObservation(_lastLidarAngle);
        sensor.AddObservation(transform.rotation.eulerAngles.y);
        */
    }

    private void AddRaycastTest(Vector3 forward, VectorSensor sensor)
    {
        RaycastHit hit;
        float maxDist = 40f;
        if (Physics.Raycast(transform.position, forward, out hit, maxDist))
        {
            sensor.AddObservation(hit.distance / maxDist);
            bool IsDust = hit.collider.tag == _dustTag;
           // sensor.AddObservation(IsDust?1f:-1f);
            Debug.DrawRay(transform.position, hit.point - transform.position , IsDust? Color.green : Color.blue);
        }
        else
        {
            sensor.AddObservation(1f);
           // sensor.AddObservation(0);
            Debug.DrawRay(transform.position, forward * maxDist, Color.red);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var forwardSpeed = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        var rotation = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);
        var rot = transform.localRotation;
        var eularRot = rot.eulerAngles;
        eularRot.y += rotation * _maxRotation;
        rot.eulerAngles = eularRot;
        transform.localRotation = rot;

        _currentSpeed += forwardSpeed * _acceleration;
        _currentSpeed = Mathf.Clamp(_currentSpeed, -_MaxBackSpeed, _MaxSpeed);

        var forward = transform.forward;
        forward.y = 0;
        transform.localPosition = transform.localPosition + forward * _currentSpeed;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == _wallTag)
        {
            //Hit wall reset
            SetReward(-2f);
            EndEpisode();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == _dustTag)
        {
            SetReward(1f);
            if(_dustSpawner.RemoveDust(other.gameObject))
            {
                SetReward(500f / (Time.time - _startTime));
                EndEpisode();
            }
        }
    }

    public void SetVC()
    {
        _lastPos = transform.position = transform.parent.transform.position + new Vector3(UnityEngine.Random.Range(-8f, 8f), 0.15f, UnityEngine.Random.Range(-8f, 8f));
        transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(-180f, 180f), 0);
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.velocity = Vector3.zero;
        _currentSpeed = 0;
    }

    public void SetResetParameters()
    {
        _rewardAddUp = 0;
        transform.parent.GetComponent<DustSpawner>().GenerateDust();
        _startTime = Time.time;
        SetVC();
    }
}
