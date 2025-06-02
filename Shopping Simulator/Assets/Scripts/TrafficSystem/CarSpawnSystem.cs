using CryingSnow.CheckoutFrenzy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawnSystem : MonoBehaviour
{
    [SerializeField, Tooltip("An array of car prefabs to spawn.")]
    private CarAI[] carPrefabs;

    [SerializeField]
    private Waypoint nearestWayPoint;

    [SerializeField, Tooltip("The minimum time between car spawns.")]
    private float minSpawnTime = 3f;

    [SerializeField, Tooltip("The maximum time between car spawns.")]
    private float maxSpawnTime = 10f;

    [SerializeField,Tooltip("Araçlarýn baþlangýçtaki rotation Y deðeri")] 
    private float rotationY;

    private IEnumerator Start()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            if (!CarCountControl.Instance.CarSpawn())
            {
                yield return new WaitUntil(() => CarCountControl.Instance.CarSpawn());
            }

            int carIndex = Random.Range(0, carPrefabs.Length);
            var car = Instantiate(carPrefabs[carIndex], transform.position,Quaternion.Euler(0,rotationY,0));
            CarCountControl.Instance.IncreaseCarNumber();
            car.nearestWayPoint = nearestWayPoint;
            car.Play();
        }
    }

}
