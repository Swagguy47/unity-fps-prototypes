using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class RopeInstance : MonoBehaviour
{
    [HideInInspector] public RopeCleat A, B;
    [HideInInspector] public SpringJoint RopeA = null, RopeB = null;
    [HideInInspector] public int UsedEnds = 1;
    [HideInInspector] public bool HoldingA, HoldingB;
    LineRenderer RopeRenderer;

    private void Start()
    {
        RopeRenderer = GetComponent<LineRenderer>();
        RopeA = null; RopeB = null;
    }

    private void LateUpdate()
    {
        if (A != null && B != null) //Rope trailrenderer
        {
            RopeRenderer.SetPosition(0, A.transform.position);
            RopeRenderer.SetPosition(1, B.transform.position);
        }
    }

    public void RefreshRope()
    {
        if (B != null)
        {
            RopeA.connectedBody = B.Base;
        }
        if (A != null)
        {
            RopeB.connectedBody = A.Base;
        }
        if (RopeA != null && RopeB != null)
        {
            RopeA.maxDistance = 5;
            RopeA.enableCollision = true;
            RopeA.anchor = A.transform.localPosition;
            RopeA.connectedAnchor = B.transform.localPosition;
            RopeA.connectedMassScale = RopeA.connectedBody.mass / 5;
        }
        if (RopeB != null && RopeA != null)
        {
            RopeB.maxDistance = 5;
            RopeB.enableCollision = true;
            RopeB.anchor = B.transform.localPosition;
            RopeB.connectedAnchor = A.transform.localPosition;
            RopeB.connectedMassScale = RopeB.connectedBody.mass / 5;
        }
    }
}
