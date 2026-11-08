using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

//логика тасков
public class ProgressManager : MonoBehaviour
{
    [SerializeField] GameObject progressObject;
    [SerializeField] int tasksCount;
    private List<GameObject> tasks;

    private bool isPosted;


    public GameObject SosketSystem;
    public GameObject ContrataptionZoneData;
    public GameObject SideLampCamera;
    public GameObject PC_Camera;

    void Start()
    {
        tasks = progressObject.GetComponent<ProgressObject>().tasks;
    }

    //проверка тасков
    void Update()
    {
        foreach (var e in tasks)
        {
            if (e.TryGetComponent(out ITask task) && !task.isResolved)
            {
                if (task.TaskCondition(this))
                {
                    task.isResolved = true;
                    progressObject.GetComponent<ProgressObject>().NextPage();

                    var i = 0;
                    foreach (var x in tasks)
                    {
                        if (x.TryGetComponent(out ITask printTask))
                        i++;
                    }
                }
                if (!task.isResolved) break;
            }
        }


    }

    //проверка выполнены ли все таски
    bool CheckTasksComplete()
    {
        foreach (var e in tasks)
        {
            if (e.TryGetComponent(out ITask task))
            {
                if (!task.isResolved)
                {
                    return false;
                }
            }
        }

        return true;
    }

    //рестарт тасков
    public void ResetProgress()
    {
        progressObject.GetComponent<ProgressObject>().StartPage();

        foreach (var e in tasks)
        {
            if (e.TryGetComponent(out ITask task) && task.isResolved)
            {
                task.isResolved = false;
                task.ResetTask();
                var i = 0;
                foreach (var x in tasks)
                {
                    if (x.TryGetComponent(out ITask printTask))
                    i++;
                }
            }
        }
    }
}
