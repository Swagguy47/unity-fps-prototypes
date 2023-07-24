using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

// a cheaper, non-rigidbody based buoyancy mimic

public class FakeBuoyancy : MonoBehaviour
{
    WaterSurface Water;
    [SerializeField] float SurfaceOffset = 0, SimDistance = 35;

    WaterSearchParameters WaterSearch;
    WaterSearchResult WaterSearchResult;

    private void Start()
    {
        //RB = GetComponent<Rigidbody>();
        Water = (WaterSurface)GameObject.FindObjectOfType(typeof(WaterSurface));
    }

    private void LateUpdate()
    {
        //Stops fake Buoyancy if camera out of sim range
        if ((Camera.main.transform.position - transform.position).magnitude < SimDistance) {
            //Finds water surface position
            WaterSearch.startPosition = transform.position;
            Water.FindWaterSurfaceHeight(WaterSearch, out WaterSearchResult);

            //Sets position to surface
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, WaterSearchResult.height + SurfaceOffset, transform.position.z), 5 * Time.deltaTime);
        }
    }
}
