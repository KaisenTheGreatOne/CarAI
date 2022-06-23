using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeBrain : MonoBehaviour
{
    public NeuralNetwork SNeuralNetwork;
    private Umgebungserkennung SUmgebung;
    private AIDrive SAiDrive;

    private float[] input = new float[4];
    public int position;

    public float distanceTraveled;
    private Vector3 lastPoint;

    [SerializeField] string currentWeights;

    private List<string> gateNames;

    void Start()
    {
        SUmgebung = gameObject.GetComponent<Umgebungserkennung>();
        SAiDrive = gameObject.GetComponent<AIDrive>();
        GetWeights();

        gateNames = new List<string>();
    }

    public bool collided;//To tell if the car has crashed

    void FixedUpdate()//FixedUpdate is called at a constant interval
    {
        if (!collided)
        {
            input[0] = SUmgebung.GetDistanceFront();
            input[1] = SUmgebung.GetDistanceFR();
            input[2] = SUmgebung.GetDistanceFL();

            input[3] = SAiDrive.GetSpeed();


            float[] output = SNeuralNetwork.FeedForward(input);

            SAiDrive.SetInpX(output[0]);
            SAiDrive.SetInpY(output[1]);

            distanceTraveled += Vector3.Distance(gameObject.transform.position, lastPoint);

            lastPoint = gameObject.transform.position;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("CheckPoint"))
        {
            if (collided)
            {
                return;
            }

            foreach (string item in gateNames)
            {
                if (other.gameObject.name == item)
                {
                    position--;
                    return;
                }
            }

            position++;

            gateNames.Add(other.gameObject.name);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        collided = true;
    }

    public void UpdateFitness()
    {
        SNeuralNetwork.fitness = position;//updates fitness of network for sorting
    }

    private void GetWeights()
    {
        currentWeights = SNeuralNetwork.GetWeights();
    }
}
