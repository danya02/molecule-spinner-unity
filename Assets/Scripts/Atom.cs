using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    static int AtomCollisionSignificantDigits = 3;

    static float RoundNumber(float original)
    {
        return (float)Math.Round((double)original, AtomCollisionSignificantDigits);
    }

    static Vector3 RoundAtomPosition(Vector3 original)
    {
        return new Vector3(
            RoundNumber(original.x),
            RoundNumber(original.y),
            RoundNumber(original.z)
            );
    }

    // Start is called before the first frame update
    void Start()
    {
    }


    public int IndexInsideCell;
    public AtomType myType;
    public List<GameObject> AllSiblings;
    public HashSet<Vector3> AllAtomPositions;
    public HashSet<int> MySiblings = new HashSet<int>();
    Dictionary<int, Vector3> offsets = new Dictionary<int, Vector3>();
    public Molecule Molecule;

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

    public void CalculateLinksToNeighbors()
    {
        MySiblings = new HashSet<int>();
        offsets = new Dictionary<int, Vector3>();
        for (int i=0; i<AllSiblings.Count; i++)
        {
            if (i == IndexInsideCell) { continue; }
            GameObject obj = AllSiblings[i];
            Atom a = obj.GetComponent<Atom>();
            if(a.MySiblings.Contains(i) && MySiblings.Contains(i)){ continue; }
            if (a.myType.covalentRadius + myType.covalentRadius -0.5 >= Vector3.Distance(transform.position, a.transform.position))
            {
                MySiblings.Add(i);
                a.MySiblings.Add(IndexInsideCell);
                offsets[i] = a.transform.position - transform.position;
                a.offsets[IndexInsideCell] = transform.position - a.transform.position;
                continue;
            }

            Vector3 X = Molecule.cell.CellToWorld(new Vector3(1, 0, 0));
            Vector3 Y = Molecule.cell.CellToWorld(new Vector3(0, 1, 0));
            Vector3 Z = Molecule.cell.CellToWorld(new Vector3(0, 0, 1));
            bool c(Vector3 offset) { return Vector3.Distance(transform.position, a.transform.position + offset) -0.5 <= a.myType.covalentRadius + myType.covalentRadius; }
            void ok(Vector3 offset) {
                Debug.DrawLine(transform.position, a.transform.position+offset, Color.blue, 1000000, false);
                a.MySiblings.Add(IndexInsideCell);
                MySiblings.Add(i);
                offsets[i] = a.transform.position - transform.position + offset;
                a.offsets[IndexInsideCell] = 
                    transform.position - a.transform.position - offset;
            }
            if(c(Vector3.zero)) { ok(Vector3.zero); continue; }
            if (c(X)) { ok(X); continue; }
            if (c(Y)) { ok(Y); continue; }
            if (c(Z)) { ok(Z); continue; }

            if (c(-X)) { ok(-X); continue; }
            if (c(-Y)) { ok(-Y); continue; }
            if (c(-Z)) { ok(-Z); continue; }

            if (c(X + Y)) { ok(X+Y); continue; }
            if (c(Y + Z)) { ok(Y+Z); continue; }
            if (c(Z + X)) { ok(Z+X); continue; }

            if (c(-X + Y)) { ok(-X+Y); continue; }
            if (c(-Y + Z)) { ok(-Y+Z); continue; }
            if (c(-Z + X)) { ok(-Z+X); continue; }

            if (c(X - Y)) { ok(X-Y); continue; }
            if (c(Y - Z)) { ok(Y-Z); continue; }
            if (c(Z - X)) { ok(Z-X); continue; }

            if (c(-X - Y)) { ok(-X-Y); continue; }
            if (c(-Y - Z)) { ok(-Y-Z); continue; }
            if (c(-Z - X)) { ok(-Z-X); continue; }

            Vector3 off(int bitmask)
            {
                Vector3 res = Vector3.zero;
                if ((bitmask & 1) == 0) { res += X; } else { res -= X; }
                if ((bitmask & 2) == 0) { res += Y; } else { res -= Y; }
                if ((bitmask & 4) == 0) { res += Z; } else { res -= Z; }
                return res;
            }

            for(int bit=0; bit<=0b111; bit++)
            {
                if (c(off(bit))) { ok(off(bit)); break; }
            }



        }
    }

    /*public void CreateLinksToNeighbors(List<GameObject> ao, GameObject stickPrefab)
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

    }*/

    void BecomeCopyOf(Atom what)
    {
        AllAtomPositions = what.AllAtomPositions;
        AllSiblings = what.AllSiblings;
        offsets = what.offsets;
        MySiblings = what.MySiblings;
        gameObject.transform.parent = what.transform.parent;
        foreach(int ind in MySiblings)
        {
            Debug.DrawRay(transform.position, AllSiblings[ind].transform.position, Color.red, 5);
        }
    }

    internal void GenerateSiblings()
    {
        foreach (int siblingInd in MySiblings)
        {
            Debug.Log(siblingInd.ToString()+offsets.ContainsKey(siblingInd).ToString());
            if(AllAtomPositions.Contains(RoundAtomPosition(offsets[siblingInd] + transform.position))) { Debug.Log("Sibling " + siblingInd.ToString() + " already exists, skipping"); continue; }
            GameObject sibling = Instantiate(AllSiblings[siblingInd]);
            sibling.transform.position = offsets[siblingInd] + transform.position;
            sibling.GetComponent<Atom>().BecomeCopyOf(AllSiblings[siblingInd].GetComponent<Atom>());
            AllAtomPositions.Add(RoundAtomPosition(sibling.transform.position));
            Debug.Log("Sibling " + siblingInd.ToString() + " instantiated");
            Debug.DrawLine(transform.position, sibling.transform.position, Color.red);
        }
    }

    public void OnDestroy()
    {
        if (transform != null && AllAtomPositions != null)
        {
            AllAtomPositions.Remove(transform.position);
        }
    }
}
