using DG.Tweening;
using SymbolRecognition;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Drob : MonoBehaviour
{
    [System.Serializable]
    public struct DefeatResult
    {
        public Symbol spell;
        public Sprite sprite;
    }

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<DefeatResult> spellResults;
    //[SerializeField] private Dictionary<Symbol, Sprite> spellResults;

    private Collider collider;
    private Sequence sequence;
    private Transform mainCameraTransform;

    private static List<Drob> aliveDrobs = new List<Drob>();

    // ---------- Unity methods

    private void Awake()
    {
        collider = GetComponent<Collider>();
        mainCameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        transform.LookAt(mainCameraTransform.position, transform.parent.up);
    }

    private void OnDestroy()
    {
        sequence.Kill();
    }

    // ---------- public methods

    public void Initialize(Sequence sequence)
    {
        this.sequence = sequence;
        aliveDrobs.Add(this);
    }

    public void Trafiony(Symbol spell)
    {
        DefeatResult result = spellResults.Find(sr => sr.spell == spell);
        spriteRenderer.sprite = result.sprite;
        collider.enabled = false;

        //TODO - Make drob stop moving and/or attack
        sequence.Pause();
        aliveDrobs.Remove(this);
    }

    public void ChangeSortingLayer(int newSortingLayer)
    {
        spriteRenderer.sortingLayerID = newSortingLayer;
    }

    // ---------- public static methods

    public static bool IsAllDrobDefeated()
    {
        return aliveDrobs.Count == 0;
    }
}
