using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class AtomOnVRClick: MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<VRHandLaserPointer>().PointerClick += AtomOnVRClick_PointerClick;
    }

    private void AtomOnVRClick_PointerClick(object sender, PointerEventArgs e)
    {
        Atom target = e.target.GetComponent<Atom>();
        if (target != null)
        {
            target.enabled = true;
            target.GenerateCellUnitsInMySiblings();
            target.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
