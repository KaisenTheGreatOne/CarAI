using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AIDrive : MonoBehaviour
{
    [Header("AI")]
    [SerializeField] private Transform RayOrigin;
    [SerializeField] private float distanceF;
    [SerializeField] private float distanceR;
    [SerializeField] private float distanceL;
    [SerializeField] private float connectionDistanceFGas;
    [SerializeField] private float connectionDistanceFBreak;
    [SerializeField] private float connectionDistanceRSteer;
    [SerializeField] private float connectionDistanceLSteer;

    [Header("Physics")]
    private Rigidbody PhRb;
    [SerializeField] private Vector3 PhRbCenterOfMass;

    [Header("Inputs")]
    [SerializeField] private float inpX;
    [SerializeField] private float inpY;
    [SerializeField] private float inpRev;

    [Header("Driving Wheels")]
    [SerializeField] private bool dwIsForwardSlip;
    [SerializeField] private bool dwIsSidewaySlip;
    [SerializeField] private float dwFowardSlip;
    [SerializeField] private float dwSideSlip;
    [SerializeField] private float dwRpm;
    [SerializeField] private bool dwIsRWD;
    [SerializeField] private float dwPower;
    [SerializeField] private float dwBreak;
    [SerializeField] private float dwEngineBreak;
    [SerializeField] private float dwBreakBias;
    [SerializeField] private float[] dwBreakTorques;
    [SerializeField] private WheelCollider[] ODWDrivingWheelsR;
    [SerializeField] private WheelCollider[] ODWDrivingWheelsL;

    [Header("WheelObjects")]
    [SerializeField] private GameObject[] OWOWheelObjectsR;
    [SerializeField] private GameObject[] OWOWheelObjectsL;
    [SerializeField] private GameObject OWOWheelR;
    [SerializeField] private GameObject OWOWheelL;

    [Header("Steering Wheels")]
    [SerializeField] private float swMaxSteerAnge;
    [SerializeField] private float swCurrentSteerangle;
    [SerializeField] private WheelCollider[] OSWSteeringWheels;

    [Header("Car Status")]
    [SerializeField] private float csSpeedFactor;
    [SerializeField] private float csSpeed;
    [SerializeField] private bool csInPitLane;
    [SerializeField] private float csCurrentTireStatus;

    [Header("Dirtyness")]
    [SerializeField] private bool dwOnGrass;
    [SerializeField] private float dwDirtyness;
    [SerializeField] private float dwTireCleaningFactor;
    [SerializeField] private GameObject ODirtPatch;
    [SerializeField] private Material PatchMat;

    void Start()
    {
        
    }

    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(RayOrigin.position, transform.forward, out hit))
        {
            distanceF = Vector3.Distance(RayOrigin.position, hit.point);
        }

        RaycastHit hitR;
        Vector3 vectorR = Quaternion.AngleAxis(45, Vector3.up) * transform.forward;
        if (Physics.Raycast(RayOrigin.position, vectorR, out hitR))
        {
            distanceR = Vector3.Distance(RayOrigin.position, hitR.point);
        }

        RaycastHit hitL;
        Vector3 vectorL = Quaternion.AngleAxis(-45, Vector3.up) * transform.forward;
        if (Physics.Raycast(RayOrigin.position, vectorL, out hitL))
        {
            distanceL = Vector3.Distance(RayOrigin.position, hitL.point);
        }

        Debug.DrawRay(RayOrigin.position, transform.forward * 10, Color.yellow);
        Debug.DrawRay(RayOrigin.position, vectorR * 20, Color.green);
        Debug.DrawRay(RayOrigin.position, vectorL * 20, Color.red);

        if (distanceR < distanceL)
        {
            if (swCurrentSteerangle > -30)
            {
                swCurrentSteerangle -= 3 * ((distanceL - distanceR) * 0.01f);
            }
        }
        else
        {
            if (swCurrentSteerangle < 30)
            {
                swCurrentSteerangle += 3 * ((distanceR - distanceL) * 0.01f);
            }
        }

        if (distanceL > 20 && distanceR > 20)
        {
            swMaxSteerAnge = 0;
        }

        Steer();
        Drive();
    }


    private void GetTireInfo()
    {
        WheelHit hit = new WheelHit();

        foreach (WheelCollider wheels in ODWDrivingWheelsR)
        {
            if (wheels.GetGroundHit(out hit))
            {
                dwFowardSlip = hit.forwardSlip;
                dwSideSlip = hit.sidewaysSlip;

                if (hit.collider.gameObject.CompareTag("Grass"))
                {
                    dwOnGrass = true;
                    dwDirtyness = hit.collider.material.staticFriction;
                }
                else
                {
                    float rpm = Mathf.Abs(wheels.rpm);

                    dwOnGrass = false;
                    dwDirtyness += Time.deltaTime * rpm / dwTireCleaningFactor;
                }

                WheelFrictionCurve fFriction = wheels.forwardFriction;
                fFriction.stiffness = dwDirtyness;
                wheels.forwardFriction = fFriction;
                WheelFrictionCurve sFriction = wheels.sidewaysFriction;
                sFriction.stiffness = dwDirtyness;
                wheels.sidewaysFriction = sFriction;

                if (dwFowardSlip < -0.5)
                {
                    dwIsForwardSlip = true;
                }
                else if (dwFowardSlip > 0.5)
                {
                    dwIsForwardSlip = true;
                }
                else
                {
                    dwIsForwardSlip = false;
                }
            }
        }

        foreach (WheelCollider wheels in ODWDrivingWheelsL)
        {
            if (wheels.GetGroundHit(out hit))
            {
                dwFowardSlip = hit.forwardSlip;
                dwSideSlip = hit.sidewaysSlip;

                if (hit.collider.gameObject.CompareTag("Grass"))
                {
                    dwOnGrass = true;
                    dwDirtyness = hit.collider.material.staticFriction;
                }
                else
                {
                    float rpm = Mathf.Abs(wheels.rpm);

                    dwOnGrass = false;
                    dwDirtyness += Time.deltaTime * rpm / dwTireCleaningFactor;
                }

                WheelFrictionCurve fFriction = wheels.forwardFriction;
                fFriction.stiffness = dwDirtyness;
                wheels.forwardFriction = fFriction;
                WheelFrictionCurve sFriction = wheels.sidewaysFriction;

                if (dwFowardSlip < -0.5)
                {
                    sFriction.stiffness = sFriction.asymptoteValue;
                    dwIsForwardSlip = true;
                }
                else if (dwFowardSlip > 0.5)
                {
                    sFriction.stiffness = sFriction.asymptoteValue;
                    dwIsForwardSlip = true;
                }
                else
                {
                    sFriction.stiffness = dwDirtyness;
                    dwIsForwardSlip = false;
                }

                wheels.sidewaysFriction = sFriction;
            }
        }
    }

    private void Steer()
    {
        foreach (WheelCollider wheels in OSWSteeringWheels)
        {
            wheels.steerAngle = swCurrentSteerangle;
        }
    }

    private void Drive()
    {
        Accelerate();
    }

    private void SetPitLimiter(WheelCollider wheels)
    {
        if (wheels.gameObject.name.Contains("H"))
        {
            wheels.motorTorque = 0;
            wheels.brakeTorque = dwBreak;
        }
    }

    private void Accelerate()
    {
        dwBreakTorques[0] = -inpY * dwBreak * dwBreakBias;
        dwBreakTorques[1] = -inpY * dwBreak * (1 - dwBreakBias);

        if (inpRev != 0)
        {
            if (inpY >= 0)
            {
                foreach (WheelCollider wheels in ODWDrivingWheelsR)
                {
                    SetWheelTorqueReverse(wheels);
                }

                foreach (WheelCollider wheels in ODWDrivingWheelsL)
                {
                    SetWheelTorqueReverse(wheels);
                }
            }
            else
            {
                foreach (WheelCollider wheels in ODWDrivingWheelsR)
                {
                    SetWheelBreak(wheels);
                }

                foreach (WheelCollider wheels in ODWDrivingWheelsL)
                {
                    SetWheelBreak(wheels);
                }
            }
        }
        else
        {
            if (inpY > 0)
            {
                foreach (WheelCollider wheels in ODWDrivingWheelsR)
                {
                    SetWheelTorque(wheels);
                }

                foreach (WheelCollider wheels in ODWDrivingWheelsL)
                {
                    SetWheelTorque(wheels);
                }

            }
            else if (inpY < 0)
            {
                foreach (WheelCollider wheels in ODWDrivingWheelsR)
                {
                    SetWheelBreak(wheels);
                }

                foreach (WheelCollider wheels in ODWDrivingWheelsL)
                {
                    SetWheelBreak(wheels);
                }
            }
            else
            {
                foreach (WheelCollider wheels in ODWDrivingWheelsR)
                {
                    SetWheelEngineBreak(wheels);
                }

                foreach (WheelCollider wheels in ODWDrivingWheelsL)
                {
                    SetWheelEngineBreak(wheels);
                }
            }
        }
    }

    private void SetWheelTorque(WheelCollider wheels)
    {
        wheels.brakeTorque = 0;

        if (dwIsRWD)
        {
            if (wheels.gameObject.name.Contains("H"))
            {
                wheels.motorTorque = inpY * dwPower;
            }
        }
        else
        {
            wheels.motorTorque = inpY * dwPower;
        }
    }

    private void SetWheelTorqueReverse(WheelCollider wheels)
    {
        wheels.brakeTorque = 0;

        if (dwIsRWD)
        {
            if (wheels.gameObject.name.Contains("H"))
            {
                wheels.motorTorque = inpRev * dwPower;
            }
        }
        else
        {
            wheels.motorTorque = inpRev * dwPower;
        }
    }

    private void SetWheelBreak(WheelCollider wheels)
    {
        if (wheels.gameObject.name.Contains("H"))
        {
            wheels.motorTorque = 0;
            wheels.brakeTorque = -inpY * dwBreak * (1 - dwBreakBias);
        }
        else
        {
            wheels.motorTorque = 0;
            wheels.brakeTorque = -inpY * dwBreak * dwBreakBias;
        }
    }

    private void SetWheelEngineBreak(WheelCollider wheels)
    {
        wheels.motorTorque = 0;

        if (dwIsRWD)
        {
            if (wheels.gameObject.name.Contains("H"))
            {
                wheels.brakeTorque = dwEngineBreak;
            }
        }
        else
        {
            wheels.brakeTorque = dwEngineBreak;
        }
    }

    private void UpdateWheelPoses()
    {
        for (int i = 0; i < OWOWheelObjectsR.Length; i++)
        {
            Vector3 _posR;
            Quaternion _rotR;
            Vector3 _posL;
            Quaternion _rotL;

            ODWDrivingWheelsR[i].GetWorldPose(out _posR, out _rotR);
            ODWDrivingWheelsL[i].GetWorldPose(out _posL, out _rotL);

            OWOWheelObjectsR[i].transform.position = _posR;
            OWOWheelObjectsL[i].transform.position = _posL;

            OWOWheelObjectsR[i].transform.rotation = _rotR;
            OWOWheelObjectsL[i].transform.rotation = _rotL;
        }
    }

    private void LateUpdate()
    {
        if (dwDirtyness >= csCurrentTireStatus)
        {
            dwDirtyness = csCurrentTireStatus;
        }

        UpdateWheelPoses();
    }
}