using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class BuoyancyNew : MonoBehaviour
{
    Rigidbody RB;
    WaterSurface Water;
    public float SubmergedDepth = 1, DisplacementAmount = 3, WaterDrag = 2, WaterAngularDrag = 2, SimDistance = 80;
    [SerializeField] bool Drifts, AvoidKinematic, IgnoreSimDst;
    [SerializeField] [Header("Floater Positions")] Transform[] Floaters;

    WaterSearchParameters WaterSearch;
    WaterSearchResult WaterSearchResult;

    private void Start()
    {
        RB = GetComponent<Rigidbody>();
        Water = (WaterSurface)GameObject.FindObjectOfType(typeof(WaterSurface));
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, Camera.main.transform.position) < SimDistance || IgnoreSimDst)
        {
            if (!AvoidKinematic) {
                RB.isKinematic = false;
            }
            foreach (Transform Floater in Floaters)
            {
                RB.AddForceAtPosition(Physics.gravity / Floaters.Length, Floater.position, ForceMode.Acceleration);

                //Finds water surface position
                WaterSearch.startPosition = Floater.position;
                Water.FindWaterSurfaceHeight(WaterSearch, out WaterSearchResult);

                if (Floater.position.y < WaterSearchResult.height)
                {
                    float DisplacementMult = Mathf.Clamp01(WaterSearchResult.height - Floater.position.y / SubmergedDepth) * DisplacementAmount;

                    //Gravity
                    RB.AddForceAtPosition(new Vector3(0, Mathf.Abs(Physics.gravity.y) * DisplacementMult, 0), Floater.position, ForceMode.Acceleration);
                    //Drag
                    RB.AddForce(DisplacementMult * -RB.velocity * WaterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
                    //Angular drag
                    RB.AddTorque(DisplacementMult * -RB.angularVelocity * WaterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
                    //Drifts with wind
                    if (Drifts)
                    {
                        RB.AddForceAtPosition((Vector3.right / 3) / Floaters.Length, Floater.position, ForceMode.Acceleration);
                    }
                }
            }
        }
        else if (!AvoidKinematic)
        {
            RB.isKinematic = true;
        }
    }
}
