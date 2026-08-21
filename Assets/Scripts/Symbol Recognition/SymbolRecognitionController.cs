using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Unity.Collections;
using TMPro;

namespace SymbolRecognition
{
    public class SymbolRecognitionController : MonoBehaviour
    {
        [System.Serializable]
        private struct SymbolA
        {
            public Symbol symbol;
            public bool active;
        }

        [SerializeField]
        private bool active;

        [SerializeField]
        [Tooltip("List of available symbols to be drawn by the system")]
        private SymbolA[] symbolsList;

        [SerializeField]
        [Tooltip("Game Object representing Particle drawn when drawing a symbol")]
        private GameObject spellParticle;

        private List<SymbolParticle> particlePool = new List<SymbolParticle>();
        private int currentParticleIndex = 0;

        [SerializeField]
        [Tooltip("Number of particles to be spawned in pool by the system")]
        private int particleNumber;

        [SerializeField]
        [Tooltip("A distance between each drawn particle")]
        private float particleDistance;

        [SerializeField]
        [Tooltip("Minimum number of particles/directions for the direction to be registered in the final symbol")]
        private int minParticleThresh = 2;

        [SerializeField, Min(1)]
        [Tooltip("Minimum number of particles in a pool to flash symbols after being drawn")]
        private int flashParticleNumber;
        [SerializeField]
        [Tooltip("If true then a symbol is flashed from the used points when drawn correctly")]
        private bool flashDrawnSymbol = true;

        private List<SymbolParticle> flashPartPool = new List<SymbolParticle>();
        private int currentFlashPartIndex = 0;

        private Vector3 lastParticle;

        [SerializeField]
        [Tooltip("A cursor that will draw the symbol to recognize (Scene Game Object only)")]
        private Transform drawingCursor;

        [Space]

        [SerializeField]
        [Tooltip("If to assign Input callbacks on Awake() in this script (for Start and Stop drawing)")]
        private bool useInputSystemHere = false;

        [SerializeField]//ShowIf("useInputSystemHere", true)
        [Tooltip("Change input to suit your needs, do not change names or types of inputs")]
        private InputActionMap actionMap;

        [Space]

        [SerializeField]
        [Tooltip("An event that is invoked when a recognizable and active symbol is drawn")]
        private SymbolEvent symbolDrawnEvent;

        [SerializeField]
        private Texture2D cursorWhenDrawing;
        [SerializeField]
        private Texture2D cursorWhenNotDrawing;

        [Header("Debug")]// --------------------------------------------------- Debug stuff (or stuff displayed when debug is enabled)

        [SerializeField]
        [Tooltip("Turn on to show debug options (and Log messages)")]
        private bool debug = false;

        [SerializeField, ReadOnly]//ShowIf("debug", true)
        private bool drawing;

        [SerializeField, ReadOnly]//ShowIf("debug", true)
        [Tooltip("Currently drawn symbol (Read only)")]
        private List<Direction> currentSymbol = new List<Direction>();

        [SerializeField, ReadOnly]//ShowIf("debug", true)
        [Tooltip("A list of particles in the currently drawn symbol (used when flashing the symbol is enabled)")]
        private List<Vector3> particlesPositions = new List<Vector3>();

        [SerializeField, ReadOnly]//ShowIf("debug", true)
        [Tooltip("Last drawn symbol (with proportions)")]
        private List<SymbolLine> lastDrawnSymbol = new List<SymbolLine>();

        [SerializeField]//ShowIf("debug", true)
        private TMP_Text debugTextMesh;

        private Vector2 vector225 = new Vector2(414.2135624f, -1000);
        private Transform particleParent = null;

        public uint playerId { get; private set; }

        // ---------- Unity messages

        void Awake()
        {
            //this is not used for now (checkbox in Inspector is disabled)
            if (useInputSystemHere)
            {
                actionMap["DrawingTrigger"].started += (ctx) => { InitiateDrawingSrv(); /*Cursor.SetCursor(cursorWhenDrawing, Vector2.zero, CursorMode.Auto);*/ };
                actionMap["DrawingTrigger"].canceled += (ctx) => { FinishedDrawing(); /*Cursor.SetCursor(cursorWhenNotDrawing, Vector2.zero, CursorMode.Auto);*/ };
            }
        }

        private void Start()
        {
            GenerateParticlePool();

            //Cursor.SetCursor(cursorWhenNotDrawing, Vector2.zero, CursorMode.Auto);
        }

        void OnEnable()
        {
            actionMap.Enable();
        }

        void OnDisable()
        {
            actionMap.Disable();
        }

        void Update()
        {
            if (drawing)
            {
                if (!active)
                {
                    drawing = false;
                    return;
                }

                //generate new particle when moved with enough distance
                if (Vector3.Distance(drawingCursor.localPosition, lastParticle) >= particleDistance)
                {
                    //float angle = Vector2.SignedAngle(vector225, drawingCursor.position - lastParticle) + 180;

                    if (debug && debugTextMesh != null)
                    {
                        //debugTextMesh.text = "Angle: " + (Vector2.SignedAngle(vector225, drawingCursor.position - lastParticle) + 180).ToString();
                        debugTextMesh.text = "Current direction: " + ((Direction)((Vector2.SignedAngle(vector225, drawingCursor.localPosition - lastParticle) + 180) / 45)).ToString();
                    }

                    currentSymbol.Add((Direction)((Vector2.SignedAngle(vector225, drawingCursor.localPosition - lastParticle) + 180) / 45));

                    lastParticle = drawingCursor.localPosition;

                    //drawing/rendering of particles on Clients (also takes flashing into account)
                    DrawParticleRpc(lastParticle);
                    

                    //lastParticle = Instantiate(spellParticle, drawingCursor.position, Quaternion.identity).transform.position;
                }
            }
        }

        // ---------- public methods

        public void Enable()
        {
            GenerateParticlePool();

            active = true;
        }

        public void Disable()
        {
            active = false;
        }

        public void SetCursor(Transform cursorTransform)
        {
            drawingCursor = cursorTransform;
        }

        public void SetParticleParent(Transform _particleParent)
        {
            particleParent = _particleParent;

            GenerateParticlePool();
        }

        //wrapper called from NetworkPlayer
        public void StartDrawing()
        {
            InitiateDrawingSrv();
        }

        //wrapper called from DuelManager
        //[Server]
        public void SrvStopDrawing()
        {
            FinishedDrawing();
        }

        // ---------- private methods

        //Used to Initialize drawing
        //[Server]
        private void InitiateDrawingSrv()
        {
            if (!active)
                return;

            lastParticle = drawingCursor.localPosition;

            //particle drawing/rendering on Clients
            DrawParticleRpc(lastParticle);

            //lastParticle = Instantiate(spellParticle, drawingCursor.position, Quaternion.identity).transform.position;

            currentSymbol.Clear();
            lastDrawnSymbol.Clear();

            InitializeDrawingRpc();

            drawing = true;
        }

        //[ClientRpc]
        private void InitializeDrawingRpc()
        {
            if (flashDrawnSymbol)
                particlesPositions.Clear();
        }

        //Used when finished drawing (Server)
        private void FinishedDrawing()
        {
            if (!active)
                return;

            drawing = false;

            //collect directions of drawn symbol
            if (currentSymbol.Count > 0)//If symbol exists recognizes directions used in it (with lenghts)
            {
                SymbolLine newLine;
                newLine.direction = currentSymbol[0];
                newLine.length = 1;
                for (int i = 1; i < currentSymbol.Count; i++)
                {
                    if (currentSymbol[i] == newLine.direction)//if next particle direction is the same
                    {
                        newLine.length++;
                        continue;
                    }
                    else if (newLine.length >= minParticleThresh)//when new direction reached -> check if last direction is long enough to count
                    {
                        lastDrawnSymbol.Add(newLine);
                    }

                    newLine.direction = currentSymbol[i];
                    newLine.length = 1;
                }

                if (newLine.length >= minParticleThresh)//when finished particles check if last direction can be added
                    lastDrawnSymbol.Add(newLine);
            }

            //debug block
            if (debug && debugTextMesh != null)
            {
                string symbols = "";

                for (int i=0; i<lastDrawnSymbol.Count; i++)
                {
                    symbols += lastDrawnSymbol[i].direction.ToString() + "/" + lastDrawnSymbol[i].length.ToString() + " ";
                }    

                debugTextMesh.text = "Symbol drawn: " + symbols;
            }

            //checking if symbol drawn matches any of the symbols in database
            if (lastDrawnSymbol.Count > 0)
            {
                bool symbolFound = false;

                for (int i=0; i<symbolsList.Length; i++)
                {
                    if (symbolsList[i].active && lastDrawnSymbol.Count == symbolsList[i].symbol.lines.Length)
                    {
                        Symbol checkedSymbol = symbolsList[i].symbol;
                        bool directMatch = true;
                        for (int j=0; j<lastDrawnSymbol.Count; j++)
                        {
                            if (lastDrawnSymbol[j].direction != checkedSymbol.lines[j].direction)
                            {
                                directMatch = false;
                                break;
                            }
                        }

                        if (directMatch)
                        {
                            //when symbol found a match
                            symbolFound = true;
                            FlashSymbolRpc();
                            symbolDrawnEvent.Invoke(checkedSymbol);

                            //Send Symbol to Spell System
                            //SpellSystem.instance.CastSymbolSrv(checkedSymbol, playerId);

                            break;
                        }
                    }
                }

                if (debug && !symbolFound)
                {
                    Debug.Log("Drawn symbol could not be found in the database");
                }
            }
        }

        //Generates at least two particles in the normal and flash pools
        //[Button, DisableInEditorMode]
        private void GenerateParticlePool()
        {
            if (particleNumber < 2)
                particleNumber = 2;

            for (int i=0; i<particlePool.Count; i++)
                Destroy(particlePool[i].gameObject);

            particlePool.Clear();

            for (int i = 0; i < particleNumber; i++)
                particlePool.Add(Instantiate(spellParticle, particleParent).GetComponent<SymbolParticle>());

            currentParticleIndex = 0;

            if (flashDrawnSymbol)
            {
                if (flashParticleNumber < 2)
                    flashParticleNumber = 2;

                for (int i = 0; i < flashPartPool.Count; i++)
                    Destroy(flashPartPool[i].gameObject);

                flashPartPool.Clear();

                for (int i = 0; i < flashParticleNumber; i++)
                    flashPartPool.Add(Instantiate(spellParticle, particleParent).GetComponent<SymbolParticle>());

                currentFlashPartIndex = 0;
            }
        }

        //Flashes a symbol when drawn correctly (if this option is enabled)
        //[ClientRpc]
        private void FlashSymbolRpc()
        {
            if (flashDrawnSymbol)
            {
                for (int i=0; i<particlesPositions.Count; i++)
                {
                    currentFlashPartIndex++;
                    if (currentFlashPartIndex >= flashPartPool.Count)
                        currentFlashPartIndex = 0;

                    flashPartPool[currentFlashPartIndex].ResetParticle(particlesPositions[i]);
                }
            }
        }

        //[ClientRpc]
        private void DrawParticleRpc(Vector3 localPosition)
        {
            currentParticleIndex++;
            if (currentParticleIndex >= particlePool.Count)//TO DO - make object pooling reset current particle for ease of "blinkin" after successfull drawing
                currentParticleIndex = 0;
            particlePool[currentParticleIndex].ResetParticle(localPosition);

            if (flashDrawnSymbol)
                particlesPositions.Add(localPosition);
        }

        // ---------- public methods

        public void SetPlayerId(uint newId)
        {
            playerId = newId;
        }
    }

    [System.Serializable]
    public class SymbolEvent : UnityEvent<Symbol>
    { }
}