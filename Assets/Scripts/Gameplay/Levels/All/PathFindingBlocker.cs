using System.Collections.Generic;
using UnityEngine;

namespace PathFinding
{
    public abstract class PathFindingBlocker : MonoBehaviour
    {
        private static List<PathFindingBlocker> blockers = new List<PathFindingBlocker>();

        public static List<PathFindingBlocker> GetPathFindingBlockers() => blockers;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The list of cell where the blocker block the pathFinder algorithme, note that (0,0) is the left down corner of the map</returns>
        public abstract List<MapPoint> GetBlockedCells();

        protected virtual void Awake()
        {
            blockers.Add(this);
        }
    }

}

