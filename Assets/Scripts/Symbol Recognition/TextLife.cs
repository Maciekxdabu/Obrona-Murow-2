using TMPro;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextLife : MonoBehaviour
{
    [SerializeField]
    private float lifeTime = 1;

    [SerializeField, ReadOnly]
    private float livedFor = 0;

    private TMP_Text textMesh;
    private Color currColor;

    private void Awake()
    {
        textMesh = GetComponent<TMP_Text>();

        currColor = textMesh.color;
    }

    void Update()
    {
        livedFor += Time.deltaTime;

        if (livedFor >= lifeTime)
            Destroy(gameObject);
        else
        {
            currColor.a = 1 - livedFor / lifeTime;
            textMesh.color = currColor;
        }
    }
}
