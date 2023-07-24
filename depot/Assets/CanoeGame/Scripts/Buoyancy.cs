using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Buoyancy : MonoBehaviour
{
    [SerializeField] Rigidbody RB;
    WaterSurface Water;
    [SerializeField] float SubmergedDepth = 1, DisplacementAmount = 3, WaterDrag = 2, WaterAngularDrag = 2;
    [SerializeField] [Header("Floater Positions")] int Floaters;

    WaterSearchParameters WaterSearch;
    WaterSearchResult WaterSearchResult;

    private void Start()
    {
        //RB = GetComponent<Rigidbody>();
        Water = (WaterSurface)GameObject.FindObjectOfType(typeof(WaterSurface));
    }

    private void FixedUpdate()
    {
        RB.AddForceAtPosition(Physics.gravity / Floaters, transform.position, ForceMode.Acceleration);

        //Finds water surface position
        WaterSearch.startPosition = transform.position;
        Water.FindWaterSurfaceHeight(WaterSearch, out WaterSearchResult);

        if (transform.position.y < WaterSearchResult.height) {
            float DisplacementMult = Mathf.Clamp01(WaterSearchResult.height- transform.position.y / SubmergedDepth) * DisplacementAmount;

            //Gravity
            RB.AddForceAtPosition(new Vector3(0, Mathf.Abs(Physics.gravity.y) * DisplacementMult, 0), transform.position, ForceMode.Acceleration);
            //Drag
            RB.AddForce(DisplacementMult * -RB.velocity * WaterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
            //Angular drag
            RB.AddTorque(DisplacementMult * -RB.angularVelocity * WaterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }

    //Editor floater gizmos
    /*private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        foreach (Vector3 Floater in Floaters) {
            Gizmos.DrawSphere(transform.position + Floater, 0.1f);
        }
    }*/
}
