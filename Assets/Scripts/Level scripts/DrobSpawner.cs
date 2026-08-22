using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class DrobSpawner : MonoBehaviour
{
    public enum DrobType
    {
        Kaczka,
        Ges,
        Kura,
    }

    [System.Serializable]
    public struct SinglePath
    {
        public List<Vector3> pathPoints;
        public float duration;
        public Color debugColor;
    }

    [SerializeField] private GameObject drobPrefab;
    [SerializeField] private DrobType drobType = DrobType.Kaczka;
    [SerializeField] public List<SinglePath> path;//its public for Editor tool

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

    private void OnDrawGizmosSelected()
    {
        path.ForEach(singlePath =>
        {
            Gizmos.color = singlePath.debugColor; ;
            Gizmos.DrawLineStrip(singlePath.pathPoints.ToArray(), false);
        });
    }

    // ---------- public methods

    public Drob SpawnDrob()
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

            return newDrob;
        }

        return null;
    }

    public DrobType GetDrobType()
    {
        return drobType;
    }
}
