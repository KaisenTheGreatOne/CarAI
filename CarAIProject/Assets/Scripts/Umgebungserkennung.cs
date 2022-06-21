using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIDrive))]
public class Umgebungserkennung : MonoBehaviour
{
    [Header("AI")]
    [SerializeField] private Transform RayOrigin;
    [SerializeField] private float distanceF;
    [SerializeField] private float distanceR;
    [SerializeField] private float distanceL;

    public RaycastHit GetRayInfoFront()
    {
        RaycastHit hit;

        if (Physics.Raycast(RayOrigin.position, transform.forward, out hit))
        {
            return hit;
        }

        Debug.DrawRay(RayOrigin.position, transform.forward * 10, Color.yellow);

        return hit;
    }

    public RaycastHit GetRayInforFR()
    {
        RaycastHit hitR;
        Vector3 vectorR = Quaternion.AngleAxis(45, Vector3.up) * transform.forward;

        if (Physics.Raycast(RayOrigin.position, vectorR, out hitR))
        {
            return hitR;
        }

        Debug.DrawRay(RayOrigin.position, vectorR * 20, Color.green);

        return hitR;
    }

    public RaycastHit GetRayInforFL()
    {
        RaycastHit hitL;
        Vector3 vectorL = Quaternion.AngleAxis(-45, Vector3.up) * transform.forward;

        if (Physics.Raycast(RayOrigin.position, vectorL, out hitL))
        {
            return hitL;
        }

        Debug.DrawRay(RayOrigin.position, vectorL * 20, Color.red);

        return hitL;
    }

    public float GetDistanceFront()
    {
        return Vector3.Distance(RayOrigin.position, GetRayInfoFront().point);
    }

    public float GetDistanceFR()
    {
        return Vector3.Distance(RayOrigin.position, GetRayInforFR().point);
    }

    public float GetDistanceFL()
    {
        return Vector3.Distance(RayOrigin.position, GetRayInforFL().point);
    }

}
