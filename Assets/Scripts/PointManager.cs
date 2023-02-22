using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointManager : MonoBehaviour
{

    List<GameObject> pointList = new List<GameObject>();
    public GameObject point;

    public int numOfPointStart = 20;

    // Start is called before the first frame update
    void Start()
    {
        // randomly generates points around the arena each game

        for (int i = 0; i < numOfPointStart; i++)
        {
            pointList.Add(point.gameObject);
            Instantiate(pointList[i], new Vector3(Random.Range(-45.0f, 45.0f), 4.0f, Random.Range(-45.0f, 45.0f)), pointList[i].transform.rotation);
        }

        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    
}
