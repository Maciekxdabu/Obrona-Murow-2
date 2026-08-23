using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

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
        [SortingLayer]
        public int sortingLayer;
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
            Gizmos.color = singlePath.debugColor;
            Vector3[] drawnPoints = singlePath.pathPoints.ToArray();
            transform.TransformPoints(drawnPoints);
            Gizmos.DrawLineStrip(drawnPoints, false);
        });
    }

    // ---------- public methods

    public Drob SpawnDrob()
    {
        //Spawn drob
        Drob newDrob = Instantiate(drobPrefab, transform).GetComponent<Drob>();

        if (newDrob)
        {
            //Create a Sequence
            Sequence sequence = DOTween.Sequence();
            path.ForEach(singlePath =>
            {
                sequence.AppendCallback(() => newDrob.ChangeSortingLayer(singlePath.sortingLayer));
                sequence.Append(newDrob.transform.DOLocalPath(singlePath.pathPoints.ToArray(), singlePath.duration));
                
            });
            sequence.AppendCallback(() =>
            {
                LevelController.Instance.OnDrobUBram();
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
