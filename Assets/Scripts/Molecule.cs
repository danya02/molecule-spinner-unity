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

    public static AtomType FromName(string name)
    {
        return new AtomType(name);
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

    private string[] ShelxlCommands = new string[]
    {
        "ABIN", "ACTA", "AFIX", "ANIS", "ANSC", "ANSR", "BASF",
        "BIND", "BLOC", "BOND", "BUMP", "CELL", "CGLS", "CHIV",
        "CONF", "CONN", "DAMP", "DANG", "DEFS", "DELU", "DFIX",
        "DISP", "EADP", "END",  "EQIV", "EXTI", "FEND", "FLAT",
        "FMAP", "FRAG", "FREE", "FVAR", "GRID", "HFIX", "HKLF",
        "HTAB", "ISOR", "LATT", "LAUE", "LIST", "MERG", "MORE",
        "MOVE", "MPLA", "NCSY", "NEUT", "OMIT", "PART", "PLAN",
        "PRIG", "REM",  "RESI", "RIGU", "RTAB", "SADI", "SAME",
        "SFAC", "SHEL", "SIMU", "SIZE", "SPEC", "STIR", "SUMP",
        "SWAT", "SYMM", "TEMP", "TITL", "TWIN", "TWST", "UNIT",
        "WGHT", "WIGL", "WPDB", "XNPD", "ZERR"
    };

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

    void FromOrtString(string data)
    {
        data = data.Replace("=" + System.Environment.NewLine + " ", " ");
        List<AtomType> atomTypes = new List<AtomType>();
        using (System.IO.StringReader reader = new System.IO.StringReader(data))
        {
            string line;
            while (reader.Peek()!=-1)
            {
                line = reader.ReadLine();
                if (line.StartsWith("TITL"))
                {
                    name = line.Substring(4);
                    continue;
                }
                if (line.StartsWith("SFAC"))
                {
                    atomTypes = new List<AtomType>();
                    string[] names = line.Substring(4).Split(new char[] { ' ' });
                    foreach (string name in names) { atomTypes.Add(AtomType.FromName(name)); }
                    continue;
                }
                bool isCommand = false;
                foreach (string cmd in ShelxlCommands)
                {
                    if (line.StartsWith(cmd)) { isCommand = true; break; }
                }
                if (isCommand) { continue; }
                string[] lineComponents = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                throw new System.Exception("TODO: implement atom data reading.");
            }
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
