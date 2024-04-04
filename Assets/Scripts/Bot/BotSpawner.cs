using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotSpawner : MonoBehaviour
{
    [SerializeField] private Transform _botSpawnPoint;
    [SerializeField] private Bot _botPrefab;
    [SerializeField] private Base _parentBase;

    public void Spawn(List<Bot> bots)
    {
        Vector3 spawnPosition = _botSpawnPoint.position;
        spawnPosition.y = _botPrefab.transform.position.y;

        Bot bot = Instantiate(_botPrefab, spawnPosition, Quaternion.identity);

        bot.SetParentBase(_parentBase);
        bots.Add(bot);
    }

    public void SpawnStarterBots(List<Bot> bots)
    {
        int amountOfStartingBots = 3;

        for (int i = 0; i < amountOfStartingBots; i++)
        {
            Spawn(bots);
        }
    }
}
