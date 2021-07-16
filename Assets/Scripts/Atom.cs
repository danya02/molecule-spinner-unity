using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atom : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    AtomType myType;

    public void Become(AtomDataElement dataElement, CellData cell)
    {
        this.transform.localPosition = cell.CellToWorld(dataElement.position);
        this.gameObject.name = dataElement.name;
        myType = dataElement.type;
        this.gameObject.GetComponent<Renderer>().material = dataElement.type.material;
        float scale = dataElement.type.spaceFillingRadius*2;
        //scale = dataElement.type.ballStickRadius * 2;
        this.transform.localScale = new Vector3(scale, scale, scale);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
