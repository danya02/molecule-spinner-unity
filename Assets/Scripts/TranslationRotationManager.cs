using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslationRotationManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void MoveUp()
    {
        transform.position += Vector3.up * transform.localScale.x;
    }

    public void MoveDown()
    {
        transform.position += Vector3.down * transform.localScale.x;
    }

    public void ScaleUp()
    {
        float scale = transform.localScale.x + 0.1f;
        scale = Mathf.Min(scale, 10f);
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void ScaleDown()
    {
        float scale = transform.localScale.x - 0.1f;
        scale = Mathf.Max(scale, 0.1f);
        transform.localScale = new Vector3(scale, scale, scale);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
