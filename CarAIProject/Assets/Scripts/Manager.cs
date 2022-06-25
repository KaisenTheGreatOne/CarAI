using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public Transform startPos;

    public float timeframe;
    public int populationSize;//creates population size
    public GameObject prefab;//holds bot prefab

    public int[] layers = new int[3] { 4, 3, 2 };//initializing network to the right size

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;

    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    [Range(0.1f, 10f)] public float Gamespeed = 1f;

    //public List<Bot> Bots;
    public List<NeuralNetwork> networks;
    private List<LeBrain> cars;

    void Start()// Start is called before the first frame update
    {
        if (populationSize % 2 != 0)
            populationSize = 50;//if population size is not even, sets it to fifty

        InitNetworks();
        InvokeRepeating("CreateBots", 0.1f, timeframe);//repeating function
    }

    public void InitNetworks()
    {
        networks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Load("Assets/Pre-trained.txt");//on start load the network save
            networks.Add(net);
        }
    }

    public void CreateBots()
    {
        Time.timeScale = Gamespeed;//sets gamespeed, which will increase to speed up training
        if (cars != null)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                GameObject.Destroy(cars[i].gameObject);//if there are Prefabs in the scene this will get rid of them
            }

            SortNetworks();//this sorts networks and mutates them
        }

        cars = new List<LeBrain>();
        for (int i = 0; i < populationSize; i++)
        {
            LeBrain car = (Instantiate(prefab, startPos.position, startPos.rotation)).GetComponent<LeBrain>();//create botes
            car.SNeuralNetwork = networks[i];//deploys network to each learner
            cars.Add(car);
        }

    }

    public void SortNetworks()
    {
        for (int i = 0; i < populationSize; i++)
        {
            cars[i].UpdateFitness();//gets bots to set their corrosponding networks fitness
        }
        networks = networks.OrderBy(o => o.fitness).ToList();

        networks[populationSize - 1].Save(Application.dataPath + "/Save.txt");//saves networks weights and biases to file, to preserve network performance
        for (int i = 0; i < populationSize / 2; i++)
        {
            networks[i] = networks[i + populationSize / 2].copy(new NeuralNetwork(layers));
            networks[i].Mutate((int)(1 / MutationChance), MutationStrength);
        }
    }
}