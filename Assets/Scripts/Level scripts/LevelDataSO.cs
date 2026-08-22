using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Scriptable Objects/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [System.Serializable]
    public class SegmentData
    {
        [System.Serializable]
        public class DrobStats
        {
            public DrobSpawner.DrobType type;
            public float punkty;
            public float speed;
            public float spawnDelay;
            public float spawnDelayDelta;
            public float initialSpawnDelay;

            [NonSerialized]
            public float currentTimer = 1;
            [NonSerialized]
            public List<DrobSpawner> spawners = new List<DrobSpawner>();
        }

        public float pointSum = 1000;
        public float pointDeltaStep = 50;
        public float pointDeltaStepDelta = 0;
        public List<DrobStats> drobStats = new List<DrobStats>();
    }

    public List<SegmentData> segmentsData;
}
