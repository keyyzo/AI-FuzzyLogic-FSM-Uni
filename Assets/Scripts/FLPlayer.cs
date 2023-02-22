using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FLS;
using FLS.Rules;
using FLS.MembershipFunctions;

// Fuzzy Logic Sharp library - Created by David Grupp - Adapted from https://github.com/davidgrupp/Fuzzy-Logic-Sharp 

public class FLPlayer : MonoBehaviour
{
    // Fuzzy Logic Player variables

    Rigidbody rb;

    IFuzzyEngine FindingEngine;
    IFuzzyEngine RotatingEngine;
    LinguisticVariable Finding;
    LinguisticVariable Speed;
    LinguisticVariable RotatingSpeed;
    LinguisticVariable RotatingAngle;

    // public
    public float maxTurnSpeed = 5.0f;
    public float maxMoveSpeed = 10.0f;
    public float findingRange = 50.0f;
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
    public bool isFinished = false;
    private bool scoring = false;
    private int totalPoints = 0;

    private float globalDistance = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        tempObj = point;

        // fuzzy set variables

        Finding = new LinguisticVariable("Finding");
        Speed = new LinguisticVariable("Speed");
        RotatingSpeed = new LinguisticVariable("RotatingSpeed");
        RotatingAngle = new LinguisticVariable("RotatingAngle");
        FindingEngine = new FuzzyEngineFactory().Default();
        RotatingEngine = new FuzzyEngineFactory().Default();

        // Distance to Point

        var closePoint = Finding.MembershipFunctions.AddTriangle("close", 1, 2, 5);
        var mediumPoint = Finding.MembershipFunctions.AddTriangle("medium", 5, 25, 50);
        var farPoint = Finding.MembershipFunctions.AddTriangle("far", 10, 50, 100);

        // Moving to point speed

        var fastSpeed = Speed.MembershipFunctions.AddTriangle("fastSpeed", 10, 30, 50);
        var mediumSpeed = Speed.MembershipFunctions.AddTriangle("mediumSpeed", 7.5, 15, 30);
        var slowSpeed = Speed.MembershipFunctions.AddTriangle("slowSpeed", 5, 10, 20);

        // Rotating Speed

        var rotateSpeedNone = RotatingSpeed.MembershipFunctions.AddTrapezoid("rotSpeedNone", 2, 5, 7.5, 7.5);
        var rotateSpeedSlow = RotatingSpeed.MembershipFunctions.AddTrapezoid("rotSpeedSlow", 2.5, 5, 10, 15);
        var rotateSpeedNormal = RotatingSpeed.MembershipFunctions.AddTrapezoid("rotSpeedNormal", 5, 10, 15, 25);
        var rotateSpeedFast = RotatingSpeed.MembershipFunctions.AddTrapezoid("rotSpeedFast", 10, 20, 30, 50);

        // Rotating Angle

        var rotateAngleFarLeft = RotatingAngle.MembershipFunctions.AddTrapezoid("rotAngleFarLeft", -180, -130, -80, -50);
        var rotateAngleLeft = RotatingAngle.MembershipFunctions.AddTrapezoid("rotAngleLeft", -100, -50, -25, -10);
        var rotateAngleCloseLeft = RotatingAngle.MembershipFunctions.AddTrapezoid("rotAngleCloseLeft", -30,-15, -5, -1);
        var rotateAngleNone = RotatingAngle.MembershipFunctions.AddTrapezoid("rotAngleNone", -1, -0.5,0.5, 1);
        var rotateAngleCloseRight = RotatingAngle.MembershipFunctions.AddTrapezoid("rotAngleCloseRight", 1, 5, 15, 30);
        var rotateAngleRight = RotatingAngle.MembershipFunctions.AddTrapezoid("rotAngleRight", 10, 25, 50, 100);
        var rotateAngleFarRight = RotatingAngle.MembershipFunctions.AddTrapezoid("rotAngleFarRight", 50, 80, 130, 180);

        // Finding rules

        var findingRule1 = Rule.If(Finding.Is(farPoint)).Then(Speed.Is(fastSpeed));
        var findingRule2 = Rule.If(Finding.Is(mediumPoint)).Then(Speed.Is(mediumSpeed));
        var findingRule3 = Rule.If(Finding.Is(closePoint)).Then(Speed.Is(slowSpeed));

        // Rotating rules

        var rotatingRule1 = Rule.If(RotatingAngle.Is(rotateAngleNone)).Then(RotatingSpeed.Is(rotateSpeedNone));
        var rotatingRule2 = Rule.If(RotatingAngle.Is(rotateAngleCloseRight).Or(RotatingAngle.Is(rotateAngleCloseLeft))).Then(RotatingSpeed.Is(rotateSpeedSlow));
        var rotatingRule3 = Rule.If(RotatingAngle.Is(rotateAngleRight).Or(RotatingAngle.Is(rotateAngleLeft))).Then(RotatingSpeed.Is(rotateSpeedNormal));
        var rotatingRule4 = Rule.If(RotatingAngle.Is(rotateAngleFarRight).Or(RotatingAngle.Is(rotateAngleFarLeft))).Then(RotatingSpeed.Is(rotateSpeedFast));

        FindingEngine.Rules.Add(findingRule1,findingRule2,findingRule3);
        RotatingEngine.Rules.Add(rotatingRule1, rotatingRule2, rotatingRule3, rotatingRule4);

    }

    private void FixedUpdate()
    {
        // calculates truth values for both rotation speed and movement speed

        double result = FindingEngine.Defuzzify(new {Finding = (double)globalDistance });

        double result2 = RotatingEngine.Defuzzify(new {RotatingAngle = (double)tempRotation.y });

        transform.rotation = Quaternion.Lerp(new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w), new Quaternion(0.0f, tempRotation.y, 0.0f, tempRotation.w), (float)result2 * Time.deltaTime);

        if (movingForward)
        {
            rb.velocity = transform.forward * (float)result;
        }
        
    }

    // Update is called once per frame
    void Update()
    {

        // checks if ai is ready to exit the arena

        if (totalPoints == pointsToFind)
        {
            maxPoints = true;
            scoring = false;
        }

        else
        {
            scoring = true;
        }

        // runs all ai tasks

        GetClosestPoint();
        RotateToPoint();
        ExitArena();
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
               // globalDistance = dist;

            }


        }

        point = closestPoint;
        closestPointFound = true;

        return point;
    }

    // carries out the rotation
    // by figuring out the distance between the ai position and point position
    // and using a look rotation function to rotate correctly

    void RotateToPoint()
    {
        if (scoring)
        {
            float maxDistance = Vector3.Distance(point.transform.position, transform.position);

            globalDistance = maxDistance;

            if (closestPointFound && maxDistance < findingRange)
            {

                Vector3 direction = point.transform.position - transform.position;
                Quaternion rotation = Quaternion.LookRotation(direction);
                tempRotation = rotation;
                rotating = true;

            }

            float facingAngle = Vector3.Dot((point.transform.position - transform.position).normalized, transform.forward.normalized);

            if (closestPointFound && rotating && facingAngle > 0.99999f) 
            {

                closestPointFound = false;
                rotating = false;
                movingForward = true;
            }
        }

        
    }

    // rotates the ai to face the exit gate or exit
    // then moves the ai towards it

    void ExitArena()
    {
        if (maxPoints && !isGateOpen)
        {
            GameObject gate = GameObject.FindGameObjectWithTag("RedGate");

            Vector3 direction = gate.transform.position - transform.position;
            globalDistance = Vector3.Distance(gate.transform.position,transform.position);
            Quaternion rotation = Quaternion.LookRotation(direction);
            tempRotation = rotation;
            //transform.rotation = Quaternion.Lerp(new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w), new Quaternion(0.0f, rotation.y, 0.0f, rotation.w), maxTurnSpeed * Time.deltaTime);

            float facingAngle = Vector3.Dot((gate.transform.position - transform.position).normalized, transform.forward.normalized);

            if (facingAngle > 0.7f)
            {
                movingForward = true;
            }


        }

        if (isGateOpen)
        {
            GameObject exit = GameObject.FindGameObjectWithTag("RedExit");

            Vector3 direction = exit.transform.position - transform.position;
            globalDistance = Vector3.Distance(exit.transform.position, transform.position);
            Quaternion rotation = Quaternion.LookRotation(direction);
            tempRotation = rotation;
            //transform.rotation = Quaternion.Lerp(new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w), new Quaternion(0.0f, rotation.y, 0.0f, rotation.w), maxTurnSpeed * Time.deltaTime);

            float facingAngle = Vector3.Dot((exit.transform.position - transform.position).normalized, transform.forward.normalized);

            if (facingAngle > 0.9f)
            {
                movingForward = true;
            }
        }
    }

    // collision checks
    // determines whether ai is collecting a point
    // hitting the gate or exit

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Point" && scoring)
        {
            totalPoints++;
            FindObjectOfType<FLScoreScript>().IncreaseScore();
            print(totalPoints + " red");

            point = tempObj;
            movingForward = false;
        }

        if (collision.gameObject.tag == "RedGate")
        {
            isGateOpen = true;
            rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);

            collision.gameObject.transform.position = new Vector3(collision.gameObject.transform.position.x, collision.gameObject.transform.position.y + 20.0f, collision.gameObject.transform.position.z);
        }

        if (collision.gameObject.tag == "RedExit")
        {
            rb.velocity = transform.forward * 0.0f;
            isFinished = true;
            print(isFinished);
            print("finished - red");
        }
    }

    public bool getIsFinished()
    {
        return isFinished;
    }
}
