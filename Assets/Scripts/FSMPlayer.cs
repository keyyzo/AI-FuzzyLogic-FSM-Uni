using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMPlayer : MonoBehaviour
{

    // FSM Player variables

    Rigidbody rb;

    // public
    public float maxTurnSpeed = 5.0f;
    public float maxMoveSpeed = 10.0f;
    public float findingRange = 50.0f;
    public bool scoring = false;
    public int pointsToFind = 10;

    public GameObject point;
    GameObject tempObj;

    // private

    private Quaternion tempRotation;
    private bool movingForward = false;
    private bool rotating = false;
    private bool maxPoints = false;
    private bool closestPointFound = false;
    private bool isGateOpen = false;
    private bool isFinished = false;
    
    private int totalPoints = 0;
    private Vector3 tempPointPosition;
    private enum AIState { Finding = 0, Rotating = 1, Scoring = 2, Exiting = 3, Waiting = 4 };
    private AIState aiState = AIState.Finding;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        tempObj = point;
    }

    // Called at set interval each time. Good for physics 
    void FixedUpdate()
    {
        BaseMovement();

        // switch statement controlling the active state

        switch (aiState)
        {
            case AIState.Finding:
                GetClosestPoint();
                break;
            case AIState.Rotating:
                RotateToPoint();
                break;
            case AIState.Scoring:
                ScorePoints();
                break;
            case AIState.Exiting:
                ExitArena();
                break;
            case AIState.Waiting:
                Wait();
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        

        print(aiState);
    }

    private void BaseMovement()
    {
        //Rigidbody rb = GetComponent<Rigidbody>();

        // checks if ai is ready to exit the arena

        if (totalPoints == pointsToFind)
        {
            maxPoints = true;
            scoring = false;
            aiState = AIState.Exiting;
        }

        else
        {
            scoring = true;
        }
        
    }

    // Gathers all points within the arena
    // Whichever point is closest the ai 
    // will begin to rotate towards

    GameObject GetClosestPoint()
    {
        GameObject closestPoint = null;
        float range = findingRange;

        GameObject[] objs = GameObject.FindGameObjectsWithTag("Point");
        

        foreach (GameObject pointObj in objs)
        {
            
            float dist = Vector3.Distance(pointObj.transform.position, transform.position);

            if (dist < range)
            {
                closestPointFound = true;
                
                closestPoint = pointObj;

                range = dist;
                
            }

            
        }

        point = closestPoint;
        closestPointFound = true;
        tempPointPosition = point.transform.position;
        aiState = AIState.Rotating;
        

        return point;
    }

    // carries out the rotation
    // by figuring out the distance between the ai position and point position
    // and using a look rotation function to rotate correctly
    // with a speed controller to determine how fast the rotation is

    void RotateToPoint()
    {

        if (point.transform.position == tempPointPosition)
        {
            float maxDistance = Vector3.Distance(point.transform.position, transform.position);

            if (closestPointFound && maxDistance < findingRange)
            {
                // point = pointObj;

                // range = dist;


                Vector3 direction = point.transform.position - transform.position;
                Quaternion rotation = Quaternion.LookRotation(direction);
                tempRotation = rotation;
                transform.rotation = Quaternion.Lerp(new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w), new Quaternion(0.0f, rotation.y, 0.0f, rotation.w), maxTurnSpeed * Time.deltaTime);
                rotating = true;

            }

            float facingAngle = Vector3.Dot((point.transform.position - transform.position).normalized, transform.forward.normalized);

            if (closestPointFound && rotating && facingAngle > 0.99999f) 
            {
                closestPointFound = false;
                rotating = false;
                movingForward = true;
                aiState = AIState.Scoring;
                
            }
        }

        else
        {
            closestPointFound = false;
            rotating = false;
            movingForward = false;
            aiState = AIState.Finding;
           
        }

        
    }

    // moves the ai forward 

    void ScorePoints()
    {
        if (movingForward && point.transform.position == tempPointPosition)
        {
            rb.velocity = transform.forward * maxMoveSpeed;
        }

        else
        {
            rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);
            aiState = AIState.Finding;
            
        }
        
    }

    // rotates the ai to face the exit gate or exit
    // then moves the ai towards it

    void ExitArena()
    {
        if (maxPoints && !isGateOpen)
        {
            GameObject gate = GameObject.FindGameObjectWithTag("BlueGate");

            Vector3 direction = gate.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w), new Quaternion(0.0f, rotation.y, 0.0f, rotation.w), maxTurnSpeed * Time.deltaTime);

            float facingAngle = Vector3.Dot((gate.transform.position - transform.position).normalized, transform.forward.normalized);

            if (facingAngle > 0.7f)
            {
                rb.velocity = transform.forward * maxMoveSpeed;
            }

           
        }

        if (isGateOpen)
        {
            GameObject exit = GameObject.FindGameObjectWithTag("BlueExit");

            Vector3 direction = exit.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w), new Quaternion(0.0f, rotation.y, 0.0f, rotation.w), maxTurnSpeed * Time.deltaTime);

            float facingAngle = Vector3.Dot((exit.transform.position - transform.position).normalized, transform.forward.normalized);

            if (facingAngle > 0.9f)
            {
                rb.velocity = transform.forward * maxMoveSpeed;
            }
        }
    }

    void Wait()
    {
        rb.velocity = transform.forward * 0.0f;
        rb.Sleep();
        isFinished = true;
    }

    // collision checks
    // determines whether ai is collecting a point
    // hitting the gate or exit

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Point" && scoring)
        {
            totalPoints++;
            FindObjectOfType<FSMScoreScript>().IncreaseScore();
            print(totalPoints + " blue");
            point = tempObj;
            movingForward = false;
            aiState = AIState.Finding;
        }

        if (collision.gameObject.tag == "BlueGate")
        {
            isGateOpen = true;
            rb.velocity = new Vector3(0.0f,0.0f,0.0f);

            collision.gameObject.transform.position = new Vector3(collision.gameObject.transform.position.x, collision.gameObject.transform.position.y + 20.0f, collision.gameObject.transform.position.z);
        }

        if (collision.gameObject.tag == "BlueExit")
        {
            print("finished - blue");
            isFinished = true;
            aiState = AIState.Waiting;
        }
    }

    public bool getIsFinished()
    {
        return isFinished;
    }

}


