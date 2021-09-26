using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
    }

    public new Camera camera;


    public float speed, zoomspeed;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.DownArrow))
            camera.orthographicSize -= zoomspeed;

        if (Input.GetKey(KeyCode.UpArrow))
            camera.orthographicSize += zoomspeed;

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= Vector3.right * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.position += Vector3.right * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= Vector3.up * speed * Time.deltaTime;
        }
    }
}
