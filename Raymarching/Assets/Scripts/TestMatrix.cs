using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMatrix : MonoBehaviour
{
    public Vector3 Vec = Vector3.zero;

    public Vector3 Pos;
    public Vector3 Rot;
    public Vector3 Scale = Vector3.one;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Matrix4x4 mat = Matrix4x4.identity;
        mat.SetTRS(Pos,Quaternion.Euler(Rot),Scale);

        if (Input.GetKeyDown(KeyCode.A))
        {
            Vec = mat.MultiplyPoint3x4(Vec);
            
            Debug.LogError(Vec);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Vec = mat.inverse.MultiplyPoint3x4(Vec);
            Debug.LogError(Vec);
        }
    }
}
