using System.Collections.Generic;
using UnityEngine;

namespace com.nampstudios.bumper.Zone
{
    public class Waypoint : MonoBehaviour
    {
        public List<Waypoint> connectedWaypoints = new();
    }
}