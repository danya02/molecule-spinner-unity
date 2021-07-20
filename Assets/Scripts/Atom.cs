using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class DistanceComparer : IComparer<GameObject>
{
    Vector3 position;

    public DistanceComparer(GameObject which)
    {
        position = which.transform.position;
    }
    public int Compare(GameObject a, GameObject b)
    {
        float da = Vector3.Distance(position, a.transform.position);
        float db = Vector3.Distance(position, b.transform.position);
        return (int)((da - db)*1000);
    }
}

public class Atom : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    

    public AtomType myType;

    public void Become(AtomDataElement dataElement, CellData cell, Vector3 position)
    {
        this.gameObject.tag = "Atom";
        this.transform.localPosition = cell.CellToWorld(position);
        this.gameObject.name = dataElement.name + " instance";
        myType = dataElement.type;
        foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
        {
            r.material = dataElement.type.material;
        }
        float scale = dataElement.type.ballStickRadius*2;
        this.transform.localScale = new Vector3(scale, scale, scale);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateLinksToNeighbors(List<GameObject> ao, GameObject stickPrefab)
    {
        List<Vector3> positions = new List<Vector3>();
        positions.Add(transform.position);
        List<GameObject> atomObjects = new List<GameObject>(ao);
        //atomObjects.Sort(new DistanceComparer(this.gameObject));
        foreach(GameObject g in atomObjects)
        {
            if(this.gameObject == g) { continue; }
            Atom a = g.GetComponent<Atom>();
            if (a.myType.covalentRadius + this.myType.covalentRadius >= Vector3.Distance(this.transform.position, a.transform.position))
            {
                GameObject stick = Instantiate(stickPrefab);
                stick.transform.parent = this.transform;
                
                stick.transform.position = (a.transform.position + transform.position)/2;
                stick.transform.rotation = Quaternion.LookRotation((a.transform.position - transform.position).normalized) * Quaternion.Euler(Vector3.right * 90f);
                Vector3 sv = stick.transform.localScale;
                sv.y = Vector3.Distance(this.transform.position, a.transform.position);
                stick.transform.localScale = sv;
                Debug.DrawLine(a.transform.localPosition, this.transform.localPosition, Color.blue);
            }
        }

    }
}
