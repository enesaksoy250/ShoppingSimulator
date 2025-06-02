using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Waypoint : MonoBehaviour
{
    public List<Waypoint> ConnectedWaypoints = new List<Waypoint>();
    [Tooltip("Eðer bu kavþakta trafik ýþýðý varsa, referansý buraya ekleyin.")]
    public TrafficLight LightAtPoint;
}
