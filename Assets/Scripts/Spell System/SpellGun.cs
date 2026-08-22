using SymbolRecognition;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellGun : MonoBehaviour
{
    [Header("Charges")]
    [SerializeField] private int chargesPerLoad = 5;
    [SerializeField] private List<SpellCharge> charges = new List<SpellCharge>();
    [SerializeField] private GameObject spellChargePrefab;
    [SerializeField] private Transform spellChargesParent;
    [Header("Spellcasting")]
    [SerializeField]//ShowIf("useInputSystemHere", true)
    [Tooltip("Change input to suit your needs, do not change names or types of inputs")]
    private InputActionReference spellClickAction;

    // ---------- Unity methods

    private void OnEnable()
    {
        spellClickAction.action.started += OnSpellCast;
        spellClickAction.action.Enable();
    }

    private void OnDisable()
    {
        spellClickAction.action.started -= OnSpellCast;
        spellClickAction.action.Disable();
    }

    // ---------- public methods

    public void OnSpellDrawn(Symbol newSpell)
    {
        if (newSpell == null) return;

        charges.ForEach(charge => charge.Fizzle());
        charges.Clear();

        for (int i=0; i<chargesPerLoad; i++)
        {
            SpellCharge newCharge = Instantiate(spellChargePrefab).GetComponent<SpellCharge>();
            newCharge.Initialize(newSpell);
            newCharge.transform.SetParent(spellChargesParent);
            charges.Add(newCharge);
        }
    }

    // ---------- private methods

    private void OnSpellCast(InputAction.CallbackContext ctx)
    {
        if (charges.Count > 0)
        {
            Symbol spellCast = charges[0].Spend();
            charges.RemoveAt(0);
        }
    }
}
