using UnityEngine;
using UnityEngine.InputSystem;

namespace SymbolRecognition
{
    public class CursorMovement : MonoBehaviour
    {
        [SerializeField]
        private InputAction mouseMovement;

        [SerializeField]
        private SpawnText spawnText;

        [SerializeField]
        [Tooltip("Is set by Player script/object, so here it is only a default value")]
        private float cursorSpeed;

        [SerializeField]
        private Transform cursorTransform = null;

        private Camera mainCam;
        private Mouse curMouse;
        [SerializeField]
        private SpriteMask boundingMask;

        // ---------- Unity messages

        void Awake()
        {
            mainCam = Camera.main;
            curMouse = Mouse.current;

            curMouse.WarpCursorPosition(new Vector2(mainCam.scaledPixelWidth/2, mainCam.scaledPixelHeight/2));

            mouseMovement.performed += (ctx) =>
            {
                ///method 1 (position)
                //Vector3 mousePos = mainCam.ScreenToWorldPoint(ctx.ReadValue<Vector2>());
                //mousePos.z = 0;
                //transform.position = mousePos;
                ///method 2 (position)
                //Vector3 mousePos = mainCam.ScreenToWorldPoint(curMouse.position.ReadValue());
                //mousePos.z = 0;
                //transform.position = mousePos;
                ///method 3 (delta)
                Vector3 mouseDel = ctx.ReadValue<Vector2>() * cursorSpeed;
                //Debug.Log(mouseDel);
                //mousePos.z = 0;
                cursorTransform.position += mouseDel;
                ConfineTransform();
            };
        }

        void OnEnable()
        {
            mouseMovement.Enable();
            //Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        void OnDisable()
        {
            mouseMovement.Disable();
            //Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        // ---------- public methods

        public void Initialize(SpriteMask _boundingMask)
        {
            boundingMask = _boundingMask;
        }

        public void ChangeCursorSpeed(float newSpeed)
        {
            cursorSpeed = newSpeed;
        }

        public void RandomizeCursorColor()
        {
            Color newColor = Random.ColorHSV();
            newColor.a = 1;
            if (cursorTransform != null)
                cursorTransform.GetComponent<SpriteRenderer>().color = newColor;
        }

        public Transform GetCursorTransform()
        {
            return cursorTransform;
        }

        // ---------- private methods

        private void ConfineTransform()
        {
            Vector3 pos = cursorTransform.position;
            //Vector3 tem = mainCam.ScreenToWorldPoint(Vector3.zero);
            Vector3 tem = boundingMask.bounds.min;
            if (pos.x < tem.x)
            {
                pos.x = tem.x;
                cursorTransform.position = pos;
            }
            if (pos.y < tem.y)
            {
                pos.y = tem.y;
                cursorTransform.position = pos;
            }
            //tem = mainCam.ScreenToWorldPoint(new Vector2(mainCam.scaledPixelWidth, mainCam.scaledPixelHeight));
            tem = boundingMask.bounds.max;
            if (pos.x > tem.x)
            {
                pos.x = tem.x;
                cursorTransform.position = pos;
            }
            if (pos.y > tem.y)
            {
                pos.y = tem.y;
                cursorTransform.position = pos;
            }
        }
    }
}