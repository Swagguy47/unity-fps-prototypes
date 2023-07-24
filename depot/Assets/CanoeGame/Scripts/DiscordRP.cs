using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Discord;

public class DiscordRP : MonoBehaviour
{
    private long ApplicationID = 1088668544696332409;
    [Space]
    private string details = "Placeholder Details";
    private string state = "Placeholder State";
    [Space]
    public string largeImage = "logo";
    public string largeText = "Final Voyage";

    private static Discord.Discord discord;

    private void Start()
    {
        if (discord == null)
        {
            discord = new Discord.Discord(ApplicationID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
        }
    }

    private void Update()
    {
        state = "Sector: A, Day: 1, Strike 0";
        details = "Time: 8:00 AM, Weather: Clear";

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
