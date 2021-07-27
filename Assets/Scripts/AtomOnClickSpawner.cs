using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AtomOnClickSpawner : MonoBehaviour
{
    void Start()
    {
    }

    public void OnMouseDown()
    {
        Debug.Log("Clicked: " + name);
        GetComponent<Atom>().GenerateAsymmetricUnitsInMySiblings();
        //GetComponent<Atom>().GenerateCellUnitsInMySiblings();
    }

    public void OnMouseEnter()
    {
        //GetComponent<Atom>().GenerateAsymmetricUnitsInMySiblings();

//        GetComponent<Atom>().GenerateSiblings();
    }

}