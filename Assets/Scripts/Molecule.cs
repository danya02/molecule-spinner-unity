using System.Collections.Generic;
using UnityEngine;

public class CellData
{
    public float wavelength = 0;
    public float a = 1;
    public float b = 1;
    public float c = 1;
    public float alpha_euler = 90;
    public float beta_euler = 90;
    public float gamma_euler = 90;
    public float alpha_rad { get => Mathf.Deg2Rad * alpha_euler; set => alpha_euler = Mathf.Rad2Deg * value; }
    public float beta_rad { get => Mathf.Deg2Rad * beta_euler; set => beta_euler = Mathf.Rad2Deg * value; }
    public float gamma_rad { get => Mathf.Deg2Rad * gamma_euler; set => gamma_euler = Mathf.Rad2Deg * value; }
    public float n2 { get => (Mathf.Cos(alpha_rad) - Mathf.Cos(gamma_rad) * Mathf.Cos(beta_rad)) / Mathf.Sin(gamma_rad); }

    public Matrix4x4 cellToWorldTransform { get => new Matrix4x4(
        new Vector4(a, b * Mathf.Cos(gamma_rad), c * Mathf.Cos(beta_rad), 0),
        new Vector4(0, b * Mathf.Sin(gamma_rad), c * n2, 0),
        new Vector4(0, 0, c * Mathf.Sqrt(Mathf.Pow(Mathf.Sin(beta_rad), 2) - Mathf.Pow(n2, 2)), 0),
        new Vector4(0, 0, 0, 0)
        ); }

    public Vector3 CellToWorld(Vector3 cellCoordinates)
    {
        Vector4 v = new Vector4(cellCoordinates.x, cellCoordinates.y, cellCoordinates.z);
        Vector4 newV = cellToWorldTransform * v;
        return new Vector3(newV.x, newV.y, newV.z);
    }
}

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
        "WGHT", "WIGL", "WPDB", "XNPD", "ZERR", "FMOL", "L.S.",
    };

    // Start is called before the first frame update
    void Start()
    {
        //string data = System.IO.File.ReadAllText(@"C:\Users\MSI\Desktop\f18.ort");
        string data = System.IO.File.ReadAllText(@"C:\Users\MSI\Desktop\shelx\schw44.ins");
        FromInsString(data);
        symmetries = Symmetry.ComposeAll(symmetries);
        List<Symmetry> symms = new List<Symmetry>(symmetries);
        Debug.Log(symms.Count);
        foreach(Symmetry s in symms)
        {
            Debug.Log(s.ToString());

        }
        InstantiateAtoms();

    }

    public GameObject AtomPrefab;

    public List<AtomDataElement> atoms = new List<AtomDataElement>();
    List<GameObject> atomObjects = new List<GameObject>();

    CellData cell = new CellData();
    HashSet<Symmetry> symmetries = new HashSet<Symmetry>(new SymmetryEqualityComparer());

    void InstantiateAtoms()
    {
        GameObject empty = new GameObject();
        foreach(AtomDataElement atom in atoms)
        {
            GameObject root = Instantiate(empty);
            root.transform.parent = this.transform;
            root.name = atom.name;

            List<Vector3> positions = new List<Vector3>();
            Vector3 originalPosition = atom.position;
            foreach(Symmetry symm in symmetries)
            {
                positions.Add(symm.ApplyTransform(originalPosition));
            }

            foreach (Vector3 position in positions)
            {
                GameObject atomObj = Instantiate(AtomPrefab);
                atomObj.transform.parent = root.transform;
                atomObj.GetComponent<Atom>().Become(atom, cell, position);
                atomObjects.Add(atomObj);
            }
        }

        HashSet<Vector3> seenPositions = new HashSet<Vector3>();
        GameObject cellRoot = Instantiate(empty);
        cellRoot.name = "Base Cell";


        foreach(GameObject obj in atomObjects)
        {
            if (seenPositions.Contains(obj.transform.localPosition))
            {
                Destroy(obj);
            }
            else
            {
                seenPositions.Add(obj.transform.localPosition);
                obj.transform.parent.parent = cellRoot.transform;
            }
        }

        Vector3 offsetX = cell.CellToWorld(new Vector3(1, 0, 0));
        Vector3 offsetY = cell.CellToWorld(new Vector3(0, 1, 0));
        Vector3 offsetZ = cell.CellToWorld(new Vector3(0, 0, 1));
        for(int i=0; i<2; i++)
        {
            for (int j=0; j<2; j++)
            {
                for(int k=0; k<2; k++)
                {
                    GameObject inst = Instantiate(cellRoot);
                    inst.transform.parent = cellRoot.transform;
                    inst.name = string.Format("Cell instance {0}, {1}, {2}", i, j, k);
                    inst.transform.localPosition = offsetX * i + offsetY * j + offsetZ * k;
                }
            }
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

    void FromInsString(string data)
    {
        data = data.Replace("=" + System.Environment.NewLine + " ", " ");
        List<AtomType> atomTypes = new List<AtomType>();
        atoms = new List<AtomDataElement>();
        cell = new CellData();
        symmetries = new HashSet<Symmetry>(new SymmetryEqualityComparer());
        symmetries.Add(Symmetry.identity);
        using (System.IO.StringReader reader = new System.IO.StringReader(data))
        {
            string line;
            while (reader.Peek() != -1)
            {
                line = reader.ReadLine();
                line = line.Trim();
                if(line == "") { continue; }
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

                if (line.StartsWith("CELL"))
                {
                    string[] values = line.Substring(4).Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    cell.a = float.Parse(values[1]);
                    cell.b = float.Parse(values[2]);
                    cell.c = float.Parse(values[3]);
                    cell.alpha_euler = float.Parse(values[4]);
                    cell.beta_euler = float.Parse(values[5]);
                    cell.gamma_euler = float.Parse(values[6]);
                }

                if (line.StartsWith("SYMM"))
                {
                    string[] values = line.Substring(4).Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                    Symmetry s = new Symmetry(values[0], values[1], values[2]);
                    symmetries.Add(s);
                }

                if (line.StartsWith("LATT"))
                {
                    int latticeType = int.Parse(line.Substring(4));
                    if (latticeType > 0)
                    {
                        Matrix4x4 m = Matrix4x4.zero;
                        m[0, 0] = -1;
                        m[1, 1] = -1;
                        m[2, 2] = -1;

                        symmetries.Add(new Symmetry
                        {
                            myMatrix = m
                        });
                    }
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
