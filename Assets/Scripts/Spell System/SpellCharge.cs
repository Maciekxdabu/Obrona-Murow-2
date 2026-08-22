using SymbolRecognition;
using UnityEngine;

public class SpellCharge : MonoBehaviour
{
    private Symbol spell;

    // ---------- public methods

    public void Initialize(Symbol spell)
    {
        this.spell = spell;
    }

    /// <summary>
    /// Called when trying to spend a charge to cast
    /// </summary>
    /// <returns></returns>
    public Symbol Spend()
    {
        Destroy(gameObject);
        return spell;
    }

    /// <summary>
    /// Called when removing a charge without casting (e.g. when reloading)
    /// </summary>
    public void Fizzle()
    {
        Destroy(gameObject);
    }
}
