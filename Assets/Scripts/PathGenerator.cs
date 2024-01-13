using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGenerator : MonoBehaviour
{
    //array of possible starting points
    public Transform[] initialPositions = new Transform[0];
    public GameObject pathElementPrefab;
    [Range(1.0f, 50f)]
    public float branchingRadius = 10f;
    [Range(1.0f, 5f)]
    public int branchingFactor = 3;
    private GameObject buffer;
    [SerializeField]
    [Range(0.0f, 180f)]
    private float frustrumAngle = 90f;
    [Range(1, 20)]
    public int pathLength = 7;
    private int stoneCount = 0;
    public Queue<FloatingStone> stones;
    private FloatingStone[] newLeaves;

    //First initialize the necessary variables
    //Here we use a buffer object whose transform will be useful to "build" the path
    //No need for lists or arrays, we can cache this object and reuse it
    private void Awake()
    {
        buffer = new GameObject("Buffer");
        buffer.transform.position = transform.position;
        newLeaves = new FloatingStone[branchingFactor];
    }

    private void Update()
    {
        if(stoneCount >= pathLength)
        {
            enabled = false;
        }
    }

    //This basically draws a tree, with two leaves added depending on the stone the player chose to step on
    public void GenerateBranch(Vector3 currentPosition)
    {
        //dummy buffer game object we will use to generate each tree level
        buffer.transform.position = currentPosition;
        //assuming the object linked to this script has the correct forward vector
        Quaternion baseRotation = Quaternion.LookRotation(transform.forward);
        //reset rotation before generating new stones
        float initialRotation = -90 + (180 - frustrumAngle) / 2;
        //update the buffer object's transform so turn left 90 degrees minus a small angle depending on the frustrum we chose to generate the stones
        buffer.transform.rotation = baseRotation;
        buffer.transform.Rotate(new Vector3(0, initialRotation, 0));
        //set up the angle delta 
        float spawnAngle = frustrumAngle/(branchingFactor - 1);
        //iterate for each number of children
        for (int i = 0; i < branchingFactor; i++)
        {
            //Rotate right by small amount and later spawn a stone. Do it for every stone
            Vector3 nextSpawnPosition = buffer.transform.position + buffer.transform.forward * branchingRadius;
            newLeaves[i] = GameObject.Instantiate(pathElementPrefab, nextSpawnPosition, Quaternion.identity)
                .GetComponentInChildren<FloatingStone>();
            Quaternion targetRotation = Quaternion.Euler(0, spawnAngle, 0);
            buffer.transform.Rotate(targetRotation.eulerAngles);
            FloatingStone.activeStones.Add(newLeaves[i]);
        }
        //Randomly choose one of the stones to be unstable and if the player steps on it, it will slowly dissolve and later break
        int randomObjIndex = Random.Range(0, 2);
        //one of the two stones wobbles and becomes weak
        newLeaves[randomObjIndex].MakeUnstable();
        stoneCount++;
    }

}
