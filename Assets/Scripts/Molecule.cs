using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomType
{
    public string name;
    public AtomType(string n)
    {
        name = n;
    }
}

public class AtomDataElement{
    public Vector3 position;
    public string name;
    public AtomType type;
    public AtomDataElement(Vector3 p, string n, AtomType t)
    {
        position = p;
        name = n;
        type = t;
    }
}

public class Molecule : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        AtomType at = new AtomType("Test");
        atoms.Add(new AtomDataElement(new Vector3(0, 0), "Atom1", at));
        atoms.Add(new AtomDataElement(new Vector3(0, 1), "Atom2", at));
        atoms.Add(new AtomDataElement(new Vector3(1, 0), "Atom3", at));
        atoms.Add(new AtomDataElement(new Vector3(1, 1), "Atom4", at));
        InstantiateAtoms();

    }

    public GameObject AtomPrefab;

    public List<AtomDataElement> atoms = new List<AtomDataElement>();
    List<GameObject> atomObjects = new List<GameObject>();

    void InstantiateAtoms()
    {
        foreach(AtomDataElement atom in atoms)
        {
            GameObject atomObj = Instantiate(AtomPrefab);
            atomObj.transform.SetParent(this.transform);
            atomObj.GetComponent<Atom>().Become(atom);
            atomObjects.Add(atomObj);
        }
    }

    void ResetChildren()
    {
        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }
        atomObjects = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
