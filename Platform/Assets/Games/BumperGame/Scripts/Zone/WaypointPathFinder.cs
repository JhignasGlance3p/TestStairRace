using System.Collections.Generic;
using UnityEngine;

namespace com.nampstudios.bumper.Zone
{
    public static class WaypointPathFinder
    {
        public static List<Waypoint> FindPath(Waypoint start, Waypoint target)
        {
            List<Waypoint> openSet = new() { start };
            HashSet<Waypoint> closedSet = new();
            Dictionary<Waypoint, Waypoint> cameFrom = new();
            Dictionary<Waypoint, float> gScore = new() { { start, 0 } };
            Dictionary<Waypoint, float> fScore = new() { { start, GetDistance(start, target) } };

            while (openSet.Count > 0)
            {
                Waypoint current = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (fScore[openSet[i]] < fScore[current])
                    {
                        current = openSet[i];
                    }
                }

                if (current == target)
                {
                    return RetracePath(cameFrom, current);
                }

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (var neighbor in current.connectedWaypoints)
                {
                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    float tentativeGScore = gScore[current] + GetDistance(current, neighbor);
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    else if (tentativeGScore >= gScore[neighbor])
                    {
                        continue;
                    }

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + GetDistance(neighbor, target);
                }
            }

            return null;
        }

        private static List<Waypoint> RetracePath(Dictionary<Waypoint, Waypoint> cameFrom, Waypoint current)
        {
            List<Waypoint> path = new();
            while (cameFrom.ContainsKey(current))
            {
                path.Add(current);
                current = cameFrom[current];
            }
            path.Reverse();
            return path;
        }
        private static float GetDistance(Waypoint a, Waypoint b)
        {
            return Vector3.Distance(a.transform.position, b.transform.position);
        }
    }
}