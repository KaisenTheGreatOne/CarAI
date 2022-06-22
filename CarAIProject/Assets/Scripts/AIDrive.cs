using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AIDrive : MonoBehaviour
{
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
        Steer();
        Drive();
    }

    public void SetInpX(float value)
    {
        inpX = value;
    }

    public void SetInpY(float value)
    {
        inpY = value;
    }

    public void SetInpRev(float value)
    {
        inpRev = value;
    }

    public float GetSpeed()
    {
        return csSpeed;
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
            wheels.steerAngle = swMaxSteerAnge * inpX;
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