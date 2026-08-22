using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelController : MonoBehaviour
{
    [System.Serializable]
    public class Segment
    {
        public List<DrobSpawner> spawners;

        [NonSerialized]
        public LevelDataSO.SegmentData data;
    }

    [SerializeField] private LevelDataSO levelData;
    [SerializeField] private List<Segment> segments;

    private bool active = false;
    private int activeSegmentId = -1;
    Segment activeSegment = null;
    private float currentPoints = 0f;

    // -------- Unity methods

    private void Awake()
    {
        if (levelData.segmentsData.Count != segments.Count)
        {
            Debug.LogError("Segments and SegmentsData must match!", gameObject);
            gameObject.SetActive(false);
            return;
        }

        for (int i=0; i<segments.Count; i++)
        {
            segments[i].data = levelData.segmentsData[i];
        }
    }

    void Start()
    {
        StartLevel();
    }

    void Update()
    {
        if (active)
        {
            //Segment is still ongoing
            if (currentPoints < activeSegment.data.pointSum)
            {
                //Go through the drobStats
                activeSegment.data.drobStats.ForEach(drobStat =>
                {
                    if (drobStat.spawners.Count == 0) return;

                    drobStat.currentTimer -= Time.deltaTime;

                    //Spawn drob
                    if (drobStat.currentTimer < 0)
                    {
                        drobStat.currentTimer = drobStat.spawnDelay;

                        int randomSpawnerId = Random.Range(0, drobStat.spawners.Count);
                        drobStat.spawners[randomSpawnerId].SpawnDrob();

                        currentPoints += drobStat.punkty;
                    }
                });
            }
            //Wait until all enemies are defeated
            else if (Drob.IsAllDrobDefeated())
            {
                StartNewSegment();
            }
        }
    }

    // ---------- public methods

    public void StartLevel()
    {
        currentPoints = 0f;
        StartNewSegment();

        active = true;
    }

    // ---------- private methods

    private void StartNewSegment()
    {
        Debug.Log("Zaczynamy dzisiaj nowy segment");

        activeSegmentId++;

        //Check win condition
        if (activeSegmentId >= segments.Count)
            OnWinCondition();

        //TODO - Rotate camera to a proper position before starting a segment (DOTween?)

        //Start new segment
        activeSegment = segments[activeSegmentId];
        activeSegment.data.drobStats.ForEach((drobStat) =>
        {
            drobStat.currentTimer = drobStat.initialSpawnDelay;
            drobStat.spawners = activeSegment.spawners.FindAll(spawner => spawner.GetDrobType() == drobStat.type);
        });
    }

    private void OnWinCondition()
    {
        Debug.LogWarning("Congratulations! You won!");

        gameObject.SetActive(false);

        //TODO
    }
}
