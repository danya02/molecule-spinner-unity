using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BillboardCameraTimer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FaceThis = Camera.main.transform;
    }

    Transform FaceThis;
    public static event UpdateDirectionDelegate OnUpdateTimer;

    public int FramesBetweenUpdates = 15;
    int FramesSinceLastUpdate = 0;
    // Update is called once per frame
    void Update()
    {
        FramesSinceLastUpdate += 1;
        if (FramesSinceLastUpdate >= FramesBetweenUpdates)
        {
            OnUpdateTimer.Invoke(FaceThis);
            FramesSinceLastUpdate = 0;
        }
    }


}

public delegate void UpdateDirectionDelegate(Transform lookAt);