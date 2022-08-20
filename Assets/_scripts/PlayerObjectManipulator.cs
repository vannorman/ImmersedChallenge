using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjectManipulator : MonoBehaviour
{
    public Transform hand;
    public LineRenderer lr;

    private InteractableObject lastHitObj;
    private bool objectHeld = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!objectHeld)
        {
            RaycastHit hit;
            if (Physics.Raycast(hand.transform.position, hand.transform.forward, out hit))
            {
                lr.SetPositions(new Vector3[] { hand.transform.position, hand.transform.position + hand.transform.forward * hit.distance });
                lastHitObj = hit.collider.gameObject.GetComponent<InteractableObject>();

                var reachDistance = 8f;
                if (hit.distance < reachDistance)
                {
                    if (lastHitObj && lastHitObj.PlayerMayInteract(this.GetComponent<PlayerInfo>()))
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            objectHeld = true;
                            lastHitObj.IsBeingUsed = true;
                            lastHitObj.transform.SetParent(hand.transform);
                            lastHitObj.GetComponent<Rigidbody>().isKinematic = true;
                        }

                    }
                }

                //Debug.Log("pointing atu" + hit.collider.name + ", hit dist:" + hit.distance);
            }
            else
            {
                if (lastHitObj)
                {
                    //lastHitObj.UnHighlight();
                    lastHitObj = null;
                }
                var maxLineLength = 100;
                lr.SetPositions(new Vector3[] { hand.transform.position, hand.transform.position + hand.transform.forward * maxLineLength });
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                objectHeld = false;
                lastHitObj.IsBeingUsed = false;
                lastHitObj.transform.SetParent(null);
                lastHitObj.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
       
    }
     
}
