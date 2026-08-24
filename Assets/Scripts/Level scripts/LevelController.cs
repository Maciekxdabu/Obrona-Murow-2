using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using UnityEngine.SceneManagement;
using SymbolRecognition;

[RequireComponent(typeof(AudioSource))]
public class LevelController : MonoBehaviour
{
    [System.Serializable]
    public class Segment
    {
        public List<DrobSpawner> spawners;
        public Vector3 worldRotation;
        public float rotationTime = 0.1f;

        [NonSerialized]
        public LevelDataSO.SegmentData data;
    }

    [SerializeField] private string menuSceneName;
    [SerializeField] private LevelDataSO levelData;
    [SerializeField] private Transform worldTransform;
    [SerializeField] private AudioClip gameStartClip;
    [SerializeField] private AudioClip gameFailClip;
    [SerializeField] private AudioClip gameSuccessClip;
    [SerializeField] private SpellGun spellGun;
    [SerializeField] private SymbolRecognitionController symbolRecognitionController;
    [SerializeField] private List<Segment> segments;

    private bool active = false;
    private int activeSegmentId = -1;
    private Segment activeSegment = null;
    private float currentPoints = 0f;
    private bool przegrywaszPrzegrywasz = false;

    private AudioSource audioSource;

    private static LevelController _instance = null;
    public static LevelController Instance { get { return _instance; } }

    // -------- Unity methods

    private void Awake()
    {
        _instance = this;

        audioSource = GetComponent<AudioSource>();

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
        if (active && activeSegment != null)
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

    public void OnDrobUBram()
    {
        if (przegrywaszPrzegrywasz) return;

        przegrywaszPrzegrywasz = true;

        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            Debug.LogWarning("Ohhh.... You failed...");
            spellGun.StopGunning();
            symbolRecognitionController.Disable();
            audioSource.PlayOneShot(gameFailClip);
        });
        sequence.AppendInterval(gameFailClip.length);
        sequence.AppendCallback(() =>
        {
            SceneManager.LoadScene(menuSceneName);
        });
    }

    // ---------- private methods

    private void StartNewSegment()
    {
        activeSegmentId++;
        currentPoints = 0;

        //Check win condition
        if (activeSegmentId >= segments.Count)
        {
            OnWinCondition();
            activeSegment = null;
            return;
        }

        //Rotate camera and start a new segment after it finishes
        active = false;
        activeSegment = segments[activeSegmentId];
        Sequence sequence = DOTween.Sequence();
        sequence.Append(worldTransform.transform.DORotate(activeSegment.worldRotation, activeSegment.rotationTime));
        if (activeSegmentId == 0)
        {
            sequence.AppendCallback(() => audioSource.PlayOneShot(gameStartClip));
        }

        sequence.OnComplete(OnStartNewSegment);
    }

    private void OnStartNewSegment()
    {
        activeSegment.data.drobStats.ForEach((drobStat) =>
        {
            drobStat.currentTimer = drobStat.initialSpawnDelay;
            drobStat.spawners = activeSegment.spawners.FindAll(spawner => spawner.GetDrobType() == drobStat.type);
        });

        active = true;
    }

    private void OnWinCondition()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            Debug.LogWarning("Congratulations! You won!");
            audioSource.PlayOneShot(gameSuccessClip);
        });
        sequence.AppendInterval(gameSuccessClip.length);
        sequence.AppendCallback(() =>
        {
            SceneManager.LoadScene(menuSceneName);
        });
    }
}
