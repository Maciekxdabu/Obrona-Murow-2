using Unity.Collections;
using UnityEngine;

namespace SymbolRecognition
{
    public class SymbolParticle : MonoBehaviour
    {
        [SerializeField]
        private float lifeTime = 1;

        [SerializeField, ReadOnly]
        private float livedFor = 0;

        [SerializeField]//ChildGameObjectsOnly
        private SpriteRenderer sRender;

        private Color currentColor;
        private Color baseColor;

        private void Awake()
        {
            currentColor = sRender.color;
            baseColor = sRender.color;

            livedFor = 0;

            gameObject.SetActive(false);
        }

        void Update()
        {
            livedFor += Time.deltaTime;

            if (livedFor >= lifeTime)
                gameObject.SetActive(false);
            else
            {
                currentColor.a = 1 - livedFor / lifeTime;
                sRender.color = currentColor;
            }
        }

        public void ResetParticle(Vector3 position)
        {
            transform.localPosition = position;

            currentColor = baseColor;
            livedFor = 0;

            gameObject.SetActive(true);
        }

        public void ResetParticle(Vector3 position, Color color)
        {
            ResetParticle(position);
            //TO DO - make a particle flash in a specific color
        }
    }
}