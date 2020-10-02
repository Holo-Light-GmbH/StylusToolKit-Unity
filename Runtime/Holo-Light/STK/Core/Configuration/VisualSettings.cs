using UnityEngine;

namespace HoloLight.STK.Core
{
    /// <summary>
    /// Handles the related visual settings with the Stylus Tip (Color, Size, Visibility)
    /// </summary>
    public class VisualSettings
    {
        private GameObject _tipCursor;
        /// <summary>
        /// The Stylus Ray gameObject. Currently not used. For future use
        /// </summary>
        private GameObject _tipRay;

        private HoloStylusManager _manager;


        private Vector3 _defaultScale;
        private Color _defaultColor;

        public void Init(HoloStylusManager manager)
        {
            _manager = manager;

            _tipCursor = _manager.PointerSwitcher.GetSpherePointer().gameObject;
            _tipRay = _manager.PointerSwitcher.GetRayPointer().gameObject;

            _defaultScale = _tipCursor.transform.localScale;
            _defaultColor = _tipCursor.GetComponentInChildren<MeshRenderer>().material.color;
        }

        /// <summary>
        /// Shows or Hides the Stylus Tip
        /// </summary>
        public void ToggleVisibility()
        {
            MeshRenderer _tipMeshRenderer = _tipCursor.transform.GetComponentInChildren<MeshRenderer>();
            _tipMeshRenderer.enabled = !_tipMeshRenderer.enabled;
        }

        public void SetVisibility(bool visibility)
        {
            MeshRenderer _tipMeshRenderer = _tipCursor.transform.GetComponentInChildren<MeshRenderer>();
            _tipMeshRenderer.enabled = visibility;
        }

        /// <summary>
        /// Sets the new Scale/Size for the Tip
        /// </summary>
        /// <param name="newScale">The scale of the Stylus Tip</param>
        public void SetScale(float newScale)
        {
            if (_tipCursor != null)
            {
                _tipCursor.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }

        /// <summary>
        /// Sets the Default Scale/Size of the Stylus Tip
        /// </summary>
        public void SetDefaultScale()
        {
            _tipCursor.transform.localScale = _defaultScale;
        }

        /// <summary>
        /// Sets the new Color for the Tip
        /// </summary>
        /// <param name="newColor">The new Color of the Stylus Tip</param>
        public void SetColor(Color newColor)
        {
            if (_tipCursor != null)
            {
                var rend = _tipCursor.GetComponentInChildren<MeshRenderer>();
                if (rend != null)
                {
                    rend.material.color = newColor;
                }
            }
        }

        /// <summary>
        /// Changes the Color of the Stylus Cursor to default Color
        /// </summary>
        public void SetDefaultColor()
        {
            _tipCursor.GetComponentInChildren<MeshRenderer>().material.color = _defaultColor;
        }

        /// <summary>
        /// Sets the transparency of the tip. 
        /// </summary>
        /// <param name="transparency">0.0f ... 1.0f - 0 = transparent; 1 = opaque</param>
        public void SetTransparency(float alphaTransparency)
        {
            Color currentColor = _tipCursor.GetComponent<MeshRenderer>().material.color;

            _defaultColor = new Color(currentColor.r, currentColor.g, currentColor.b, alphaTransparency);
            SetDefaultColor();
        }

        public void SetDefaultTransparency()
        {
            Color currentColor = _tipCursor.GetComponent<MeshRenderer>().material.color;

            _defaultColor = new Color(currentColor.r, currentColor.g, currentColor.b, 1);
            SetDefaultColor();
        }

        /// <summary>
        /// Changes the appearance of the Tip (maybe adjusting the scale is needed, because different meshes have different scales)
        /// </summary>
        /// <param name="newMesh"></param>
        public void SetMesh(Mesh newMesh)
        {
            _tipCursor.GetComponentInChildren<MeshFilter>().mesh = newMesh;
        }
    }
}