using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public enum Axis { X, Y }
    public Axis axis = Axis.X;
    public Transform maxLookDown;
    public Transform maxLookUp;
    //Vector3 lastMousePos = new Vector3();
    public float rotSpeed = 2;
    
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        
        var mouseDelta = new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
        // RPC this so that orientation works? Or no?
        
        if (axis == Axis.X && Mathf.Abs(mouseDelta.y) > 0 && Mathf.Abs(mouseDelta.y) < 5)
        {
            if (mouseDelta.y > 0)
            {

                // moving mouse up; try look down until max reached.
                if (Vector3.Angle(transform.forward, maxLookUp.forward) > 0.5f)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation,maxLookUp.rotation, Time.deltaTime * rotSpeed);
                }
            }
            else if (mouseDelta.y < 0)
            {
                // moving mouse up; try look down until max reached.
                if (Vector3.Angle(transform.forward, maxLookDown.forward) > 0.5f)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, maxLookDown.rotation, Time.deltaTime * rotSpeed);
                }
            }
            
        }

        if (axis == Axis.Y && mouseDelta.x != 0)
        {
            var rot2 = rotSpeed * Time.deltaTime * new Vector3(0, mouseDelta.x, 0);
            transform.Rotate(rot2, Space.World);
        }
    }
}
