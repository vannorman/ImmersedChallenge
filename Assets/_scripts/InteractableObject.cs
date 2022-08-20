using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public GameData.PlayerType[] whoMayInteract;

    private bool _IsBeingUsed = false;
    private float highlightTimer = 0f;

    public bool IsBeingUsed { get { return _IsBeingUsed; } set { _IsBeingUsed = value; } }

    internal bool PlayerMayInteract(PlayerInfo playerInfo)
    {
        if (!IsBeingUsed)
        {
            foreach (var playerType in whoMayInteract)
            {
                if (playerInfo.playerType == playerType)
                {
                    Highlight(Color.green);
                    return true;
                }
            }
            //return false;
        }
        Highlight(Color.red);
        return false;

    }

    internal void Highlight(Color color)
    {
        highlightTimer = 1f;
        this.GetComponent<Outline>().enabled = true;
        this.GetComponent<Outline>().OutlineColor = color;
    }

    internal void UnHighlight()
    {
        this.GetComponent<Outline>().enabled = false;
    }

    private void Update()
    {
        highlightTimer -= Time.deltaTime;
        if (highlightTimer < 0.5f)
        {
            var oc = this.GetComponent<Outline>().OutlineColor;
            var fadeAlphaSpeed = 2f;
            this.GetComponent<Outline>().OutlineColor = Color.Lerp(oc, Color.clear, Time.deltaTime * fadeAlphaSpeed);
        }
        if (highlightTimer < 0)
        {
            UnHighlight();
        }

    }
}
