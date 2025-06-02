using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Waypoint : MonoBehaviour
{
    public List<Waypoint> ConnectedWaypoints = new List<Waypoint>();
    [Tooltip("E�er bu kav�akta trafik ����� varsa, referans� buraya ekleyin.")]
    public TrafficLight LightAtPoint;
}
