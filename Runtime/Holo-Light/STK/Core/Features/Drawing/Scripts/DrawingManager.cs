using UnityEngine;
using HoloLight.STK.MRTK;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections.Generic;
using HoloLight.STK.Core;

namespace HoloLight.STK.Features.Drawing
{
    /// <summary>
    /// DrawingManager handles all the Drawing functions (Drawing Lines, GetLines, Remove Lines)
    /// </summary>
    public class DrawingManager : MonoBehaviour
    {
        /// <summary>
        /// Material for the lineRenderer.
        /// </summary>
        [Tooltip("Material for the lineRenderer")]
        public Material DrawingMaterial;

        /// <summary>
        /// Size of line segments (in meters) used to approximate the curve.
        /// </summary>
        [Tooltip("Size of line segments (in meters) used to approximate the curve")]
        public float LineSegmentSize = 0.002f;

        /// <summary>
        /// Thickness of the line.
        /// </summary>
        [Tooltip("Width of the line")]
        public float LineWidth = 0.01f;

        /// <summary>
        /// Color of the line.
        /// </summary>
        [Tooltip("Color of the line")]
        public Color LineColor = new Color(233, 233, 233);

        private float _lineWidthMultiplier = 1;

        [SerializeField]
        private HoloStylusManager _holoStylusManager;

        private StylusTransform _stylusTransform;

        [SerializeField]
        private InputActionHandler _inputACTIONHandler;

        private bool _isDrawing = false;

        private int _lineNr = 0;

        private LineRenderer _lineRenderer;
        private List<Vector3> _linePositions = new List<Vector3>();
        private Vector3[] linePositionsOld = new Vector3[0];

        /// <summary>
        /// The Line-GameObjects will be set as child of this Object. If not specified, then a new Parent Object will be created
        /// </summary>
        public GameObject ParentGameObject;

        public List<GameObject> LineObjects = new List<GameObject>();

        private void Awake()
        {
            if (ParentGameObject == null)
            {
                ParentGameObject = new GameObject("LineObjectsParent");
                ParentGameObject.transform.position = Vector3.zero;
            }

            _stylusTransform = _holoStylusManager.StylusTransform;
        }

        public void Activate()
        {
            SetListeners();
            _holoStylusManager.StylusTransform.SetMultiplier(18);

        }

        public void Deactivate()
        {
            RemoveListeners();
            _holoStylusManager.StylusTransform.SetDefaultMultiplier();
        }

        public void SetListeners()
        {
            _holoStylusManager.PointerSwitcher.DisablePointer(StylusPointerSwitcher.PointerType.StylusRayPointer);
            _inputACTIONHandler.enabled = true;
        }

        public void RemoveListeners()
        {
            _holoStylusManager.PointerSwitcher.EnablePointer(StylusPointerSwitcher.PointerType.StylusRayPointer);
            _inputACTIONHandler.enabled = false;

            OnHoldEnd();
        }

        public void OnHoldStarted()
        {
            _isDrawing = true;

            CreateLineRendereObject();
        }

        private void CreateLineRendereObject()
        {
            GameObject lineGO = new GameObject("Line-" + _lineNr, typeof(LineRenderer));

            lineGO.transform.parent = ParentGameObject.transform;

            Material m = new Material(DrawingMaterial);
            m.SetColor("_Color", LineColor);
            _lineRenderer = lineGO.GetComponent<LineRenderer>();
            _lineRenderer.positionCount = 0;
            _lineRenderer.startColor = LineColor;
            _lineRenderer.endColor = LineColor;
            _lineRenderer.sharedMaterial = m;
            _lineRenderer.widthMultiplier = _lineWidthMultiplier * LineWidth;
            _lineRenderer.numCapVertices = 5; // roundiness of the ends
            _lineRenderer.numCornerVertices = 5;
        }

        public void OnHoldEnd()
        {
            _isDrawing = false;
            if (_lineRenderer)
            {
                if (_lineRenderer.positionCount <= 2)
                {
                    Destroy(_lineRenderer.gameObject);
                    _linePositions.Clear();
                    return;
                }

                _lineNr++;
                LineObjects.Add(_lineRenderer.gameObject);
                _lineRenderer = null;
            }

            _linePositions.Clear();
        }

        /// <summary>
        /// Draws the positions of the line
        /// </summary>
        private void SetPositions()
        {
            Vector3[] linePositions = _linePositions.ToArray();
            //create old positions if they dont match
            if (linePositionsOld.Length != linePositions.Length)
            {
                linePositionsOld = new Vector3[linePositions.Length];
            }

            //check if line points have moved
            bool moved = false;
            for (int i = 0; i < linePositions.Length; i++)
            {
                //compare
                if (linePositions[i] != linePositionsOld[i])
                {
                    moved = true;
                }
            }

            //update if moved
            if (moved == true)
            {
                //get smoothed values
                Vector3[] smoothedPoints = LineSmoother.SmoothLine(linePositions, LineSegmentSize);

                //set line settings
                _lineRenderer.positionCount = smoothedPoints.Length;
                _lineRenderer.SetPositions(smoothedPoints);
            }
        }

        public List<GameObject> GetLines()
        {
            return LineObjects;
        }

        private void FixedUpdate()
        {
            if (_isDrawing)
            {
                Vector3 stylusPosition = _stylusTransform.Position;
                // if the mouse didn't move (mouse in same position) while holding the action button, don't draw additional positions
                if (_linePositions.Count > 0)
                {
                    if (_linePositions[_linePositions.Count - 1] == stylusPosition)
                    {
                        return;
                    }
                }

                _linePositions.Add(stylusPosition);
                SetPositions();
            }
        }

        public void Undo()
        {
            if (LineObjects.Count == 0)
            {
                return;
            }

            GameObject tmpLine = LineObjects[LineObjects.Count - 1];
            Destroy(tmpLine);
            LineObjects.RemoveAt(LineObjects.Count - 1);
        }

        public void DeleteAll()
        {
            while (LineObjects.Count > 0)
            {
                Undo();
            }
        }

        public void SetLineWidthMultiplier(float newMultiplier)
        {
            _lineWidthMultiplier = newMultiplier;
        }
    }
}