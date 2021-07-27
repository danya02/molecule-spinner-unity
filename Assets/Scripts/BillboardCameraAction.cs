using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardCameraAction : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    private void Awake()
    {
        BillboardCameraTimer.OnUpdateTimer += Sub_OnUpdateTimer;
        enabled = false;
    }

    private void Sub_OnUpdateTimer(Transform lookAt)
    {
        transform.rotation = Quaternion.LookRotation(transform.position - lookAt.position);
    }

}
