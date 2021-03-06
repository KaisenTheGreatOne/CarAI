using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIDrive))]
public class Umgebungserkennung : MonoBehaviour
{
    [Header("AI")]
    [SerializeField] private LayerMask Layers;
    [SerializeField] private Transform RayOrigin;
    [SerializeField] private float distanceF;
    [SerializeField] private float distanceR;
    [SerializeField] private float distanceL;

    private void Update()
    {
        GetRayInfoFront();
        GetRayInforFR();
        GetRayInforFL();
    }

    public RaycastHit GetRayInfoFront()
    {
        RaycastHit hit;

        if (Physics.Raycast(RayOrigin.position, transform.forward, out hit, 1000, ~Layers))
        {
            distanceF = Vector3.Distance(RayOrigin.position, hit.point);
        }

        //Debug.DrawRay(RayOrigin.position, transform.forward * 10, Color.yellow);

        return hit;
    }

    public RaycastHit GetRayInforFR()
    {
        RaycastHit hitR;
        Vector3 vectorR = Quaternion.AngleAxis(45, Vector3.up) * transform.forward;

        if (Physics.Raycast(RayOrigin.position, vectorR, out hitR, 1000, ~Layers))
        {
            distanceR = Vector3.Distance(RayOrigin.position, hitR.point);
        }

        //Debug.DrawRay(RayOrigin.position, vectorR * 20, Color.green);

        return hitR;
    }

    public RaycastHit GetRayInforFL()
    {
        RaycastHit hitL;
        Vector3 vectorL = Quaternion.AngleAxis(-45, Vector3.up) * transform.forward;

        if (Physics.Raycast(RayOrigin.position, vectorL, out hitL, 1000, ~Layers))
        {
            distanceL = Vector3.Distance(RayOrigin.position, hitL.point);
        }

        //Debug.DrawRay(RayOrigin.position, vectorL * 20, Color.red);

        return hitL;
    }

    public float GetDistanceFront()
    {
        return distanceF;
    }

    public float GetDistanceFR()
    {
        return distanceR;
    }

    public float GetDistanceFL()
    {
        return distanceL;
    }

}
