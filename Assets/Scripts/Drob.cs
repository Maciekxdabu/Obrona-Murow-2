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

    // ---------- Unity methods

    private void Awake()
    {
        collider = GetComponent<Collider>();
    }

    // ---------- public methods

    public void Initialize(Sequence sequence)
    {
        this.sequence = sequence;
    }

    public void Trafiony(Symbol spell)
    {
        DefeatResult result = spellResults.Find(sr => sr.spell == spell);
        spriteRenderer.sprite = result.sprite;
        collider.enabled = false;

        //TODO - Make drob stop moving and/or attack
        sequence.Pause();
    }
}
