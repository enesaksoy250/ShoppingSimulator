using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanSpawner : MonoBehaviour
{
    public HumanMovement[] humanPrefabs;

    [SerializeField] private Waypoint nearestWaypoint;

    public int minSpawnTime = 5;
    public int maxSpawnTime = 15;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {  
            int waitTime = Random.Range(minSpawnTime,maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            if (!HumanCountControl.Instance.HumanSpawn())
            {
                yield return new WaitUntil(() => HumanCountControl.Instance.HumanSpawn());
            }

            int random = Random.Range(0, humanPrefabs.Length);
            var human = Instantiate(humanPrefabs[random], transform.position, transform.rotation);
            human.nearestWaypoint = nearestWaypoint;
            HumanCountControl.Instance.IncreaseHumanCount();

        }
    }
}
