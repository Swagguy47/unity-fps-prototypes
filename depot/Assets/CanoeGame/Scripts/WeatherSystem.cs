using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class WeatherSystem : MonoBehaviour
{
    public float TimeMult = 1, TimeRaw;
    [HideInInspector] public float WeatherDelay = 200, StormIntensity, StormMaxIntensity;
    [HideInInspector]public int Day;
    [HideInInspector] public bool Storm;
    [SerializeField] Animator SunAnim, SearchLightsAnim;
    [SerializeField] WaterSurface Water;
    [SerializeField] Volume HDRPVolume;
    [SerializeField] ParticleSystem RainFX;
    [SerializeField] AudioSource RainSfxExterior, RainSfxInterior, WavesSfx;
    ParticleSystem.EmissionModule RainFXE;

    private void Start()
    {
        RainFXE = RainFX.emission;
    }

    void Update()
    {
        //Tracks time of day & date
        TimeRaw += (Time.deltaTime * TimeMult);
        SunAnim.SetFloat("Time", TimeRaw / 720);
        if (TimeRaw >= 720)
        {
            TimeRaw = 0;
            Day++;
            NewDay();
        }
        if (TimeRaw > 470 && TimeRaw < 715) //Night
        {
            SearchLightsAnim.SetBool("Active", true);
        }
        else //Day
        {
            SearchLightsAnim.SetBool("Active", false);
        }
        
        //Delay between storms & end of storms
        WeatherDelay -= Time.deltaTime;
        if (WeatherDelay <= 0)
        {
            WeatherDelay = Random.Range(300, 1000);
            if (Random.value > 0.5f && !Storm)
            {
                Storm = true;
                WeatherDelay = Random.Range(300, 500);
                StormMaxIntensity = Random.Range(50, 150);
                //Debug.Log("Storm starting!");
            }
            else if (Storm)
            {
                Storm = false;
                //Debug.Log("Storm over...");
            }
        }

        //Controls storms coming in and out
        if (Storm && StormIntensity < StormMaxIntensity) {
            StormIntensity += Time.deltaTime;
        }
        else if(!Storm && StormIntensity > 0) {
            StormIntensity -= Time.deltaTime;
        }

        RaycastHit hit;
        //new Vector3(transform.position.x, transform.position.y + 0.15f, transform.position.z)
        if (Physics.Raycast(PlayerCallback.PlayerBrain.transform.position, transform.TransformDirection(Vector3.up), out hit, 8f))
        { //Interior Rain Audio
            RainSfxInterior.volume = StormIntensity / StormMaxIntensity;
            RainSfxExterior.volume = 0;//StormIntensity / (StormMaxIntensity * 2);
            WavesSfx.volume = 0.3f;
        }
        else
        { //Exterior Rain Audio
            RainSfxExterior.volume = StormIntensity / StormMaxIntensity;
            RainSfxInterior.volume = 0;
            WavesSfx.volume = 1;
        }

        Water.largeWindSpeed = 17 + (10 * (StormIntensity / 150)); //Norm 17, old 25, max storm ~27 --- Wave height
        SunAnim.SetFloat("Storm", StormIntensity / 150); //Storm sun dimming
        RainFXE.rateOverTimeMultiplier = StormIntensity * 300 / 150; //Raindrop particles
    }

    private void NewDay()
    {
        Debug.Log("Day: " + Day);
    }
}
