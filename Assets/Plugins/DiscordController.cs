using System;
using UnityEngine;
using Discord;

public class DiscordController : MonoBehaviour
{
    public string applikationsID;
    public string iconeName;

    private Discord.Discord discord;

    void Start()
    {
        // Konvertera sträng-ID till ulong säkert
        if (!ulong.TryParse(applikationsID, out ulong clientID))
        {
            Debug.LogError("Ogiltigt ApplikationsID! Kontrollera att det bara innehåller siffror.");
            return;
        }

        try
        {
            discord = new Discord.Discord((long)clientID, (ulong)CreateFlags.NoRequireDiscord);
            
            var activityManager = discord.GetActivityManager();
            var activity = new Activity
            {
                Assets =
                {
                    LargeImage = iconeName
                }
            };

            activityManager.UpdateActivity(activity, (result) =>
            {
                if (result == Result.Ok)
                {
                    Debug.Log("Discord status uppdaterad!");
                }
                else
                {
                    Debug.LogError($"Misslyckades att sätta Discord status: {result}");
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Kunde inte starta Discord Game SDK: {e.Message}");
        }
    }

    void Update()
    {
        // Kör endast callbacks om Discord-instansen är aktiv
        if (discord != null)
        {
            discord.RunCallbacks();
        }
    }

    void OnApplicationQuit()
    {
        // Stäng av Discord-instansen korrekt när spelet avslutas
        if (discord != null)
        {
            discord.Dispose();
        }
    }
}