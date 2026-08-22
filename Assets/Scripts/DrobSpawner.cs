using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class DrobSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct SinglePath
    {
        public List<Vector3> pathPoints;
        public float duration;
        public Color debugColor;
    }

    [SerializeField] private GameObject drobPrefab;
    [SerializeField] private float spawnInterval = 5;
    [SerializeField] public List<SinglePath> path;//its public for Editor tool

    private float currentInterval = 1;

    // ---------- Unity messages

    private void Awake()
    {
        if (drobPrefab.TryGetComponent<Drob>(out Drob drob) == false)
        {
            Debug.LogError("The prefab must have a Drob Component", gameObject);
            gameObject.SetActive(false);
            return;
        }
    }

    private void Start()
    {
        currentInterval = spawnInterval;
    }

    private void Update()
    {
        currentInterval -= Time.deltaTime;
        if (currentInterval < 0)
        {
            SpawnDrob();
            currentInterval = spawnInterval;
        }
    }

    private void OnDrawGizmosSelected()
    {
        path.ForEach(singlePath =>
        {
            Gizmos.color = singlePath.debugColor; ;
            Gizmos.DrawLineStrip(singlePath.pathPoints.ToArray(), false);
        });
    }

    // ---------- public methods

    public void SpawnDrob()
    {
        //Spawn drob
        Drob newDrob = Instantiate(drobPrefab).GetComponent<Drob>();

        if (newDrob)
        {
            //Create a Sequence
            Sequence sequence = DOTween.Sequence();
            path.ForEach(singlePath =>
            {
                sequence.Append(newDrob.transform.DOPath(singlePath.pathPoints.ToArray(), singlePath.duration));
            });
            sequence.Play();

            newDrob.Initialize(sequence);
        }
    }
}
