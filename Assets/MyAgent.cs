using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;

public class MyAgent : Agent
{
    public GameObject startPosition;
    public GameObject obstacleMarkers;
    public GameObject target;
    public GameObject obstacle;
    public GameObject mapInstances;
    Rigidbody m_rigidbody;
    public float thrust = 20.0f;
    public float torque =  20.0f;
    float lastReward = 0.0f; // hack because there is no OnEpisodeEnd
    int successes;
    bool endEpisode;
    float queuedReward;
    List<GameObject> mapObjects = new List<GameObject>();

    public GameObject thetarget;
    
    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    void resetMap()
    {
        transform.localPosition = startPosition.transform.localPosition;
        transform.localRotation = startPosition.transform.localRotation;
        m_rigidbody.velocity = Vector3.zero;
        m_rigidbody.angularVelocity = Vector3.zero;

        foreach (GameObject c in mapObjects) { Destroy(c); }
        mapObjects.Clear();
        var cnt = 1;
        var targetSpotIdx = ((int)UnityEngine.Random.Range(1, obstacleMarkers.transform.childCount));
        //var targetSpotIdx = 1;
        foreach (Transform child in obstacleMarkers.transform)
        {
            GameObject go;
            if (targetSpotIdx == cnt)
            { go = Instantiate(target); thetarget = go; }
            else { go = Instantiate(obstacle); }
            ++cnt;
            go.transform.parent = mapInstances.transform;
            go.transform.localPosition = child.transform.localPosition;
            go.transform.localRotation = child.transform.localRotation;
            mapObjects.Add(go);
        }
    }

    public override void OnEpisodeBegin()
    {
        print(lastReward);
        lastReward = 0.0f;
        resetMap();
        queuedReward = 0.0f;
        successes = 0;
        endEpisode = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (endEpisode) { return; }
        if (other.tag == "obstacle")
        {
            queuedReward += -30.0f; //TODO this will be interesting with the goal nonlinearity....if you go fast but accurate enough a few crashes wll be accepted by the algo
            endEpisode = true; //TODO does this work correct
        }
        if (other.tag == "target")
        {
            ++successes;
            queuedReward += +10.0f * successes;
            resetMap();
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(queuedReward);
        queuedReward = 0.0f;

        //AddReward((1.0f / 100.0f) * Math.Min(1.0f/Vector3.Magnitude(startPosition.transform.localPosition - thetarget.transform.localPosition),2.0f));
        var metric = (1.0f / (100.0f * Math.Min(Vector3.Magnitude(transform.localPosition - thetarget.transform.localPosition), 5.0f))) - 1.0f / (100.0f * 5.0f);
        AddReward(metric); //* 1.0f/(float)Math.Exp((float)StepCount / 200)); //TODO is step count the current step count?
        //print(metric);

        lastReward = GetCumulativeReward(); // Must be called every time because if we end by step limit endEpisode is never set to true
        if (endEpisode) {
            EndEpisode(); // TODO does this return? //TODO is it safe to call this here?
        }

        m_rigidbody.AddRelativeForce(actions.ContinuousActions[0] * thrust, 0, 0);
        m_rigidbody.AddRelativeTorque(0, actions.ContinuousActions[1] * torque, 0, ForceMode.Force);
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<float> actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxisRaw("Vertical");
        actions[1] = Input.GetAxisRaw("Horizontal");
    }
}