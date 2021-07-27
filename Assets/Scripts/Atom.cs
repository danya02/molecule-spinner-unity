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

    public static Vector3 RoundAtomPosition(Vector3 original)
    {
        return new Vector3(
            RoundNumber(original.x),
            RoundNumber(original.y),
            RoundNumber(original.z)
            );
    }

    public Vector3 canonicalPosition { get {
            GameObject superParent = gameObject;
            Vector3 answer = RoundAtomPosition(transform.position);
            answer = transform.TransformPoint(answer);
            return answer;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }


    public int IndexInsideCell;
    public AtomType myType;
    public List<GameObject> AllSiblings;
    public AtomDict AllAliveAtoms;
    public HashSet<int> MySiblings = new HashSet<int>();
    Dictionary<int, Vector3> offsets = new Dictionary<int, Vector3>();
    public Molecule Molecule;
    public int UnitGroup;

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
        enabled = false;
    }

    public GameObject stick;

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
                var newStick = Instantiate(stick);
                newStick.transform.position = (transform.position + a.transform.position + offset) / 2;
                newStick.transform.rotation = Quaternion.LookRotation(a.transform.position + offset - transform.position, Vector3.right) * Quaternion.Euler(90, 0, 0);
                newStick.transform.localScale = new Vector3(0.2f, (a.transform.position + offset - transform.position).magnitude, 0.2f);
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

    void BecomeCopyOf(Atom what)
    {
        AllAliveAtoms = what.AllAliveAtoms;
        AllSiblings = what.AllSiblings;
        offsets = what.offsets;
        MySiblings = what.MySiblings;
        gameObject.transform.parent = what.transform.parent;
        foreach(int ind in MySiblings)
        {
            Debug.DrawRay(transform.position, offsets[ind], Color.red, 500);
            var newStick = Instantiate(stick);
            newStick.transform.position = (transform.position + transform.position + offsets[ind]) / 2;
            newStick.transform.rotation = Quaternion.LookRotation(offsets[ind], Vector3.right) * Quaternion.Euler(90, 0, 0);
            newStick.transform.localScale = new Vector3(0.2f, (offsets[ind]).magnitude, 0.2f);


        }
    }

    internal List<GameObject> GenerateSiblings()
    {
        List<GameObject> mySiblings = new List<GameObject>();
        foreach (int siblingInd in MySiblings)
        {
            if(AllAliveAtoms.ContainsKey(offsets[siblingInd] + transform.position)) {
                Debug.Log("Sibling " + siblingInd.ToString() + " already exists, skipping");
                mySiblings.Add(AllAliveAtoms[offsets[siblingInd] + transform.position]);
                continue;
            }
            GameObject sibling = Instantiate(AllSiblings[siblingInd]);
            sibling.transform.position = offsets[siblingInd] + transform.position;
            sibling.GetComponent<Atom>().BecomeCopyOf(AllSiblings[siblingInd].GetComponent<Atom>());
            AllAliveAtoms.Add(sibling.GetComponent<Atom>().canonicalPosition, sibling);
            Debug.Log("Sibling " + siblingInd.ToString() + " instantiated");
            Debug.DrawLine(transform.position, sibling.transform.position, Color.red);
            // sticks not needed here
            mySiblings.Add(sibling);
        }
        return mySiblings;
    }

    internal void GenerateAsymmetricUnitsInMySiblings()
    {
        List<GameObject> mySiblingInstances = GenerateSiblings();
        HashSet<int> UnitIndexes = new HashSet<int>();
        foreach (GameObject g in mySiblingInstances)
        {
            Atom a = g.GetComponent<Atom>();
            UnitIndexes.Add(a.UnitGroup);
        }

        foreach (int ug in UnitIndexes)
        {
            foreach (GameObject s in mySiblingInstances)
            {
                s.GetComponent<Atom>().GenerateAsymmetricUnit(ug, 0);
            }
        }

    }

    internal void GenerateCellUnitsInMySiblings()
    {
        List<GameObject> mySiblingInstances = GenerateSiblings();
        foreach(GameObject go in mySiblingInstances)
        {
            go.GetComponent<Atom>().GenerateCellUnit();
        }
    }

    private void GenerateAsymmetricUnit(int ug, int depth)
    {
        if(depth > 10) { return; }
        if (UnitGroup != ug) { return; }
        foreach (int siblingInd in MySiblings)
        {
            if (AllAliveAtoms.ContainsKey(offsets[siblingInd] + canonicalPosition))
            {
                continue;
            }
            if (AllSiblings[siblingInd].GetComponent<Atom>().UnitGroup != ug) { continue; }
            GameObject sibling = Instantiate(AllSiblings[siblingInd]);
            sibling.transform.position = offsets[siblingInd] + canonicalPosition;
            sibling.GetComponent<Atom>().BecomeCopyOf(AllSiblings[siblingInd].GetComponent<Atom>());
            AllAliveAtoms.Add(sibling.GetComponent<Atom>().canonicalPosition, sibling);
            Debug.DrawLine(transform.position, sibling.transform.position, Color.red);
            // sticks not needed here
            sibling.GetComponent<Atom>().GenerateAsymmetricUnit(ug, depth+1);
        }
    }

    private void GenerateCellUnit()
    {
        Vector3 cell_offset = RoundAtomPosition(-AllSiblings[IndexInsideCell].transform.position + transform.position);

        for (int i = 0; i < AllSiblings.Count; i++)
        {
            if(i == IndexInsideCell) { continue; }
            if (AllAliveAtoms.ContainsKey(AllSiblings[i].transform.position - cell_offset)) { continue; }
            GameObject sibling = Instantiate(AllSiblings[i]);
            sibling.transform.parent = transform.parent;
            sibling.transform.position += cell_offset;
            sibling.transform.position = RoundAtomPosition(sibling.transform.position);
            sibling.GetComponent<Atom>().BecomeCopyOf(AllSiblings[i].GetComponent<Atom>());
            AllAliveAtoms.Add(sibling.GetComponent<Atom>().canonicalPosition, sibling);
            //Debug.DrawLine(transform.position, sibling.transform.position, Color.red);

        }
    }

    public void OnDestroy()
    {
        if (transform != null && AllAliveAtoms != null)
        {
            AllAliveAtoms.Remove(canonicalPosition);
        }
    }
}
