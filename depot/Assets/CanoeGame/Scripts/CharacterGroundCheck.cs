using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class CharacterGroundCheck : MonoBehaviour
{
    CharacterBrain brain;
    int EnvCount;
    WaterSurface Water;
    WaterSearchParameters WaterSearch;
    WaterSearchResult WaterSearchResult;

    private void Start()
    {
        brain = transform.parent.GetComponent<CharacterBrain>();
        Water = (WaterSurface)GameObject.FindObjectOfType(typeof(WaterSurface));
    }

    private void FixedUpdate()
    {
        //Finds water surface position
        WaterSearch.startPosition = transform.position;
        Water.FindWaterSurfaceHeight(WaterSearch, out WaterSearchResult);

        brain.Swimming = (transform.position.y + 0.25f < WaterSearchResult.height);

        brain.Grounded = (!brain.Swimming && EnvCount > 0);
    }

    private void OnDisable() //bugfix for when player gets disabled
    {
        EnvCount = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 0)
        {
            EnvCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 0)
        {
            EnvCount--;
        }
    }
}
