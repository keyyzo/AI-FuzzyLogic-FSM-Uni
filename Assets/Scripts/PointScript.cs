using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointScript : MonoBehaviour
{

    public float rotateXVal = 5.0f;
    public float rotateYVal = 5.0f;
    public float rotateZVal = 5.0f;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // applies a rotation for better looking points

        transform.Rotate(new Vector3(rotateXVal * Time.deltaTime, rotateYVal * Time.deltaTime, rotateZVal * Time.deltaTime));
    }

    private void OnCollisionEnter(Collision collision)
    {

        // regenerates the point in a new, random location

        if (collision.gameObject.tag == "FSM" || collision.gameObject.tag == "FL")
        {

           transform.position = new Vector3(Random.Range(-45.0f, 45.0f), 4.0f, Random.Range(-45.0f, 45.0f));

         }
      

    }
}
