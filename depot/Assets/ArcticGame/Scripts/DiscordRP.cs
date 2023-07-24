using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Discord;

public class DiscordRP : MonoBehaviour
{
    private long ApplicationID = 1073756110672105593;
    [Space]
    private string details = "Placeholder Details";
    private string state = "Placeholder State";
    [Space]
    public string largeImage = "logo";
    public string largeText = "Operation Snowstorm";

    private static Discord.Discord discord;

    private Sandbox Sandboxref;

    private void Start()
    {
        if (discord == null)
        {
            discord = new Discord.Discord(ApplicationID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
        }

        Sandboxref = GameObject.FindObjectOfType<Sandbox>();
    }

    private void Update()
    {
        state = "map: " + SceneManager.GetActiveScene().name + " fps: " + Mathf.RoundToInt(1.0f / Time.deltaTime);
        if (PlayerCallback.PlayerBrain.CurrentCharBrain != null)
        {
            details = "Active Characters: " + (Sandboxref.Team0.Count + Sandboxref.Team1.Count);
        }
        else
        {
            details = "placeholder details";
        }
        

        try
        {
            discord.RunCallbacks();
        }
        catch
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        updateDiscord();
    }

    private void updateDiscord()
    {
        try
        {
            var activityManager = discord.GetActivityManager();
            var activity = new Discord.Activity()
            {
                Details = details,
                State = state,
                Assets =
                {
                    LargeImage = largeImage,
                    LargeText = largeText
                },
            };

            activityManager.UpdateActivity(activity, (res) =>
            {
                //if (res != Discord.Result.Ok) Debug.Log("Discord instance failed to connect");
            });
        }
        catch
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        state = "Application quitting";
        details = "Removing Discord instance";
        updateDiscord();

        discord.Dispose();
    }
}
