using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SpawnScaleControls : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        ShowHideAction.onStateDown += ShowHideAction_onStateDown;
        ShowHideAction.onStateUp += ShowHideAction_onStateUp;
        //enabled = false;
    }

    private void ShowHideAction_onStateUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (fromSource == ActionSource)
        {
            ScaleControls.SetActive(false);
        }
    }

    private void ShowHideAction_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (fromSource == ActionSource)
        {
            ScaleControls.SetActive(true);
            //ScaleControls.transform.parent = transform;
            ScaleControls.transform.position = transform.position;
            ScaleControls.transform.rotation = transform.rotation;
            ScaleControls.transform.localScale = transform.lossyScale;
        }
    }

    public GameObject ScaleControls;
    public SteamVR_Action_Boolean ShowHideAction;
    public SteamVR_Input_Sources ActionSource;

    // Update is called once per frame
    void Update()
    {
        
    }
}
