using TMPro;
using UnityEngine;

public class SpawnText : MonoBehaviour
{
    [SerializeField]//AssetsOnly, Required("Prefab with TMP_Text is required")
    private GameObject textToSpawn;

    public void TellWhichSymbol(SymbolRecognition.Symbol symbol)
    {
        Instantiate(textToSpawn, transform.position, Quaternion.identity).GetComponent<TMP_Text>().text = symbol.name;
    }

    public void SpawnMessage(string message)
    {
        Instantiate(textToSpawn, transform.position, Quaternion.identity).GetComponent<TMP_Text>().text = message;
    }
}
