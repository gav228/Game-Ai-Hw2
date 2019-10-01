using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class liner : MonoBehaviour
{
    LineRenderer line;
    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        DrawCircle(transform.position, 1);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void DrawCircle(Vector3 position, float radius)
    {
        line.positionCount = 51;
        line.useWorldSpace = true;
        float x;
        float z;
        float angle = 20f;

        for (int i = 0; i < 51; i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            line.SetPosition(i, new Vector3(x, 1, z) + position);
            angle += (360f / 51);
        }
    }
}
