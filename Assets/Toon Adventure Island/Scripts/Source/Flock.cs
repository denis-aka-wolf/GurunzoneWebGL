using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    float speed = 1.0f;
    float rotationSpeed = 5.0f;
    float neighbourDistance = 500.0f;
    float Gv;
    bool turning = false;
    int TurnProximity;

    Vector3 goalPos = Vector3.zero;
    GameObject FreedomBox;
    GameObject DebugPos;

    private void Awake()
    {
        FreedomBox = this.transform.parent.gameObject;
        DebugPos = FreedomBox.transform.Find("GoalPositionDebug").gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        goalPos = DebugPos.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Gv = FreedomBox.transform.parent.GetComponent<GlobalFlock>().Velocity;
        rotationSpeed = FreedomBox.transform.parent.GetComponent<GlobalFlock>().TurnSpeed;
        TurnProximity = FreedomBox.transform.parent.GetComponent<GlobalFlock>().TurnProximity;
        speed = Random.Range(Gv, Gv + 0.1f);
        transform.Translate(0, 0, Time.deltaTime * speed);
        goalPos = DebugPos.transform.position;

        if (Vector3.Distance(transform.position, goalPos) <= TurnProximity) 
        {
            turning = true;
        }
        else
            turning = false;

        if (turning)
        {
            Vector3 direction = FreedomBox.GetComponent<BoxCollider>().center;
         
            if (direction == Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, rotationSpeed * Time.deltaTime);
            }
            else 
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            }

            speed = Random.Range(10, 12);
        }

        else
            {
                if (Random.Range(0, 5) < 1) 
                    ApplyRules();
            }
    }
    void ApplyRules()
    {
        GameObject[] gos;
        gos = FreedomBox.transform.parent.GetComponent<GlobalFlock>().AllFish;

        Vector3 vcenter = Vector3.zero;
        Vector3 vavoid = Vector3.zero;
        float gSpeed = 1f;
        float dist;
        int groupSize = 0;

        foreach (GameObject go in gos)
        { 
            if (go != this.gameObject)
            {
                dist = Vector3.Distance(go.transform.position, this.transform.position);
                if (dist <= neighbourDistance)
                {
                    vcenter += go.transform.position;
                    groupSize++;

                    if (dist < 5)
                    {
                        vavoid = vavoid + (this.transform.position - go.transform.position);
                    }
                    Flock anotherFlock = go.GetComponent<Flock>();
                    gSpeed = gSpeed + anotherFlock.speed; 
                }
            }
        }

        if (groupSize > 0) //check if in any group?
        {
            vcenter = vcenter / groupSize + (goalPos - this.transform.position); //group center and speed
            speed = gSpeed / groupSize;

            Vector3 direction = (vcenter + vavoid) - transform.position;
            if (direction != goalPos)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);        
        }
    }

}
