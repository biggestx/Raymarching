using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleController : MonoBehaviour
{
    [SerializeField]
    private float Factor = 100f;

    public void Start()
    {
        Factor = Random.Range(50, 100);
    }

    private void Update()
    {
        var pos = this.transform.position;
        pos.y += Mathf.Sin(Time.realtimeSinceStartup) / Factor;
        this.transform.position = pos;
    }
}
