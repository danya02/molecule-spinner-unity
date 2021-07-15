using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomType
{

    // Atomic radius, covalent radius
    private static Dictionary<string, (float, float)> atomRadiusData = new Dictionary<string, (float, float)>
    {
        ["H"] = (0.79f, 0.32f),
        ["LI"] = (1.55f, 1.63f),
        ["BE"] = (1.12f, 0.9f),
        ["B"] = (0.98f, 0.82f),
        ["C"] = (0.91f, 0.77f),
        ["N"] = (0.92f, 0.75f),
        ["O"] = (0.88f, 0.73f),
        ["F"] = (0.84f, 0.72f),
        

    };
    public string name;
    public float covalentRadius = 2.0f;
    public float spaceFillingRadius = 1.0f;
    public float ballStickRadius = 0.3f;
    public Material material;

    public static AtomType FromName(string name)
    {
        AtomType type = new AtomType();
        type.name = name;
        Material m = Resources.Load<Material>("Atoms/Atom" + name);
        if(m == null)
        {
            Shader shader = Shader.Find("Standard");
            m = new Material(shader);
            Color c = new Color();
            Hash128 h = Hash128.Compute(name);
            float value = Mathf.InverseLerp(int.MinValue, int.MaxValue, h.GetHashCode());
            c = Color.HSVToRGB(value, 1, 0.5f);
            m.SetColor("_Color", c);
        }
        type.material = m;
        (float, float) radii;
        if(!atomRadiusData.TryGetValue(name, out radii))
        {
            radii = (0.91f, 0.77f); // Assume all unknown atoms are carbon.
        }

        type.covalentRadius = radii.Item2;
        type.spaceFillingRadius = radii.Item1;
        type.ballStickRadius = radii.Item1 / 3;


        return type;
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
        string data = System.IO.File.ReadAllText(@"C:\Users\MSI\Desktop\f18.ort");
        FromOrtString(data);
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
        atoms = new List<AtomDataElement>();
        using (System.IO.StringReader reader = new System.IO.StringReader(data))
        {
            string line;
            while (reader.Peek()!=-1)
            {
                line = reader.ReadLine();
                if (line.StartsWith("TITL"))
                {
                    this.gameObject.name = line.Substring(4);
                    continue;
                }
                if (line.StartsWith("SFAC"))
                {
                    atomTypes = new List<AtomType>();
                    string[] names = line.Substring(4).Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
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

                Vector3 position = new Vector3();
                int ati = int.Parse(lineComponents[1]) - 1;
                position.x = float.Parse(lineComponents[2]);
                position.y = float.Parse(lineComponents[3]);
                position.z = float.Parse(lineComponents[4]);

                AtomDataElement newAtom = new AtomDataElement(position, lineComponents[0], atomTypes[ati]);
                atoms.Add(newAtom);
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
