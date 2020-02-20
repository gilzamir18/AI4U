using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;
using UnityEngine.SceneManagement;
public class Manager : MonoBehaviour
{    
    public static bool stopped = false;

    public static int epCounter = 0;

    public static Manager instance;

    public int numberOfEggs = 10;

    public GameObject top;
    public GameObject bottom;
    public GameObject left;
    public GameObject right;

    public BrainManager brainManager;
    private GameObject[] agents;
    private petanim[] anims;

    public GameObject agentPrefab;
    public GameObject blockPrefab;
    public GameObject eggPrefab;

    private float eggsDelta;

    public float eggsUpdateFreq = 1.0f;

    private int currentNumberOfEggs;

    private int numberOfBlocks = 30;

    private GameObject[] blocks;

    private float sumOfEnergy;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        Manager.epCounter++;
        sumOfEnergy = 0.0f;
        eggsDelta = 0;
        stopped = false;
        instance = this;
        agents = new GameObject[brainManager.brainList.Length];
        anims = new petanim[agents.Length];

        for (int i = 0; i < brainManager.brainList.Length; i++) {
            Vector3 position = new Vector3(Random.Range(left.transform.position.x, right.transform.position.x), 
                10, Random.Range(bottom.transform.position.z, top.transform.position.z));
            agents[i] = Instantiate(agentPrefab, position, Quaternion.identity);
            agents[i].name = "agent_" + i;
            anims[i] = agents[i].GetComponent<petanim>();
            anims[i].ID = i;
            brainManager.brainList[i] = agents[i].transform.GetChild(4).GetComponent<RemoteBrain>();
        }
        brainManager.Configure();
        ProduceBlocks();
        ProduceEggs(numberOfEggs);
    }

    public GameObject[] Agents {
        get {
            return agents;
        }
    }

    private void ProduceBlocks() {
        RaycastHit hit;
        blocks = new GameObject[numberOfBlocks];
        for(int i=0;i<numberOfBlocks;i++)
        {
            Vector3 position = new Vector3(Random.Range(left.transform.position.x, right.transform.position.x), 
                0, Random.Range(bottom.transform.position.z, top.transform.position.z));
            
            //Do a raycast along Vector3.down -> if you hit something the result will be given to you in the "hit" variable
            //This raycast will only find results between +-100 units of your original"position" (ofc you can adjust the numbers as you like)
            if (Physics.Raycast (position + new Vector3 (0, 100.0f, 0), Vector3.down, out hit, 200.0f)) {
                blocks[i] = Instantiate (blockPrefab, hit.point + Vector3.up * 20, Quaternion.identity);
                Vector3 s = blocks[i].transform.localScale;
                blocks[i].transform.localScale = new Vector3(s.x - 5 * Random.Range(0.0f, 1.0f), s.y - 10 * Random.Range(0.0f, 1.0f),
                                                    s.z - 5 * Random.Range(0.0f, 1.0f));
            
                blocks[i].GetComponent<Rigidbody>().mass = Random.Range(0, 100);
            } else {
                Debug.Log ("there seems to be no ground at this position");
            }
        }
    }


    private void ProduceEggs(int numberOfEggs) {
        currentNumberOfEggs += numberOfEggs;
        for(int i=0;i<numberOfEggs;i++)
        {
            int block = Random.Range(0, numberOfBlocks);
            Vector3 pos = blocks[block].transform.position;
            Instantiate (eggPrefab, pos + Vector3.up * 6, Quaternion.identity);
        }
    }


    public void DestroyEgg(){
        currentNumberOfEggs--;
        if (currentNumberOfEggs < 0) {
            currentNumberOfEggs = 0;
        }
    }


    public float SumOfEnergy {
        get {
            return this.sumOfEnergy;
        }
    }


    private float reloadDelta = 0;
    public float reloadDelay = 0.2f;

    // Update is called once per frame
    void FixedUpdate()
    {

        if (stopped) {
            reloadDelta += Time.deltaTime;
            if (reloadDelta >= reloadDelay) {
                stopped = false;
                reloadDelta = 0.0f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return;
        }

        if (!stopped) {
            sumOfEnergy = 0;
            foreach (petanim agent in anims) {
                sumOfEnergy += agent.Energy;
            }

            if (sumOfEnergy <= 0){
                stopped = true;
                return;
            }
        }

        eggsDelta += Time.deltaTime;
        if (eggsDelta > eggsUpdateFreq) {
            int newseggs = numberOfEggs - currentNumberOfEggs;
            if (newseggs > 0) {
                ProduceEggs(newseggs);
            }
            eggsDelta = 0.0f;
        }
    }
}
