using HoloLight.STK.Core;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using UnityEngine;
using static HoloLight.STK.Core.CalibrationPreferences;

namespace HoloLight.STK.MRTK
{
    /// <summary>
    /// With this class, you can enable and disable the stylus pointers at runtime.
    /// e.g. if you need to Draw, the Ray should not be visible
    /// </summary>
    public class StylusPointerSwitcher : MonoBehaviour, IMixedRealitySourceStateHandler
    {
        public enum PointerType { None = 0, StylusRayPointer = 1, StylusCursorPointer = 4, All = 99 }

        [SerializeField]
        private PointerType _activePointers = PointerType.All;

        /// <summary>
        /// Stylus Ray Pointer
        /// </summary>
        private StylusRayPointer _stylusRayPointer;
        /// <summary>
        /// Stylus Poke Pointer
        /// </summary>
        private StylusPokePointer _stylusPokePointer;
        /// <summary>
        /// Stylus Grab Pointer
        /// </summary>
        private StylusSpherePointer _stylusSpherePointer;

        private StylusTransform _stylusTransform;

        private StylusController _stylusController;


        private int _stylusOnLeftHandSureness = 0;
        private int _stylusOnRightHandSureness = 0;
        private int _stylusNotVisibleSureness = 0;

        private HoloStylusManager _manager;

        private bool _initialized = false;

        void OnEnable()
        {
            _manager = GetComponent<HoloStylusManager>();
            CoreServices.InputSystem?.RegisterHandler<IMixedRealitySourceStateHandler>(this);
            _manager.EventManager.RegisterCallback(StylusEventType.OnStylusPreferedHandChanged, OnStylusPreferedHandChanged);
        }


        void OnDisable()
        {
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealitySourceStateHandler>(this);
            _manager.EventManager.UnRegisterCallback(StylusEventType.OnStylusPreferedHandChanged, OnStylusPreferedHandChanged);
        }

        void Init()
        {
            var pointers = new HashSet<IMixedRealityPointer>();

            // Find all valid pointers
            foreach (var inputSource in CoreServices.InputSystem.DetectedInputSources)
            {
                foreach (var pointer in inputSource.Pointers)
                {
                    if (!pointers.Contains(pointer))
                    {
                        if (pointer.GetType() == typeof(StylusRayPointer))
                        {
                            _stylusRayPointer = pointer as StylusRayPointer;
                            pointers.Add(pointer);
                        } else if (pointer.GetType() == typeof(StylusPokePointer))
                        {
                            _stylusPokePointer = pointer as StylusPokePointer;
                            pointers.Add(pointer);
                        } else if (pointer.GetType() == typeof(StylusSpherePointer))
                        {
                            _stylusSpherePointer = pointer as StylusSpherePointer;
                            pointers.Add(pointer);
                        }
                    }
                }
            }

            if (_stylusRayPointer == null)
            {
                Debug.Log("Stylus Ray Pointer is not defined. Plesae add the prefab inside the Pointers Profile.");
            }
            else
            {
                _stylusController = _stylusRayPointer.Controller as StylusController;
            }

            if (_stylusPokePointer == null)
            {
                Debug.Log("Stylus Poke Pointer is not defined. Plesae add the prefab inside the Pointers Profile.");
            } else
            {
                _stylusController = _stylusPokePointer.Controller as StylusController;
            }


            if (_stylusSpherePointer == null)
            {
                Debug.Log("Stylus Sphere Pointer is not defined. Plesae add the prefab inside the Pointers Profile.");
            }
            else
            {
                _stylusController = _stylusSpherePointer.Controller as StylusController;
            }

            if (pointers.Count >= 1)
            {
                // if atleast one type of pointer is available/defined
                ChangeType(_activePointers);
            }

            _stylusTransform = _manager.StylusTransform;

            HandleHandPointers();
            _initialized = true;
        }


        public StylusRayPointer GetRayPointer()
        {
            return _stylusRayPointer;
        }

        public StylusPokePointer GetPokePointer()
        {
            return _stylusPokePointer;
        }

        public StylusSpherePointer GetSpherePointer()
        {
            return _stylusSpherePointer;
        }

        public void DisablePointer(PointerType type)
        {

            if (type == PointerType.StylusCursorPointer)
            {
                DisableCursor();
            }
            else if (type == PointerType.StylusRayPointer)
            {
                DisableRay();
            }
            else if (type == PointerType.All)
            {
                DisableCursor();
                DisableRay();
            }
        }

        public void EnablePointer(PointerType type)
        {
            if (type == PointerType.StylusCursorPointer)
            {
                EnableCursor();
            }
            else if (type == PointerType.StylusRayPointer)
            {
                EnableRay();
            }
            else if (type == PointerType.All)
            {
                EnableRay();
                EnableCursor();
            }
        }

        public void ChangeTypeInt(int currentType)
        {
            ChangeType((PointerType)currentType);
        }

        public void ChangeType(PointerType currentType)
        {
            _activePointers = currentType;

            switch (_activePointers)
            {
                case PointerType.None:
                    DisablePointer(PointerType.All);
                    break;
                case PointerType.StylusRayPointer:
                    EnablePointer(PointerType.StylusRayPointer);
                    DisablePointer(PointerType.StylusCursorPointer);
                    break;
                case PointerType.StylusCursorPointer:
                    EnablePointer(PointerType.StylusCursorPointer);
                    DisablePointer(PointerType.StylusRayPointer);
                    break;
                case PointerType.All:
                    EnablePointer(PointerType.All);
                    break;
            }
        }

        /// <summary>
        /// Enables the Cursor (Poke- and Grabpointers)
        /// </summary>
        private void EnableCursor()
        {
           // PointerUtils.SetPointerBehavior<StylusSpherePointer>(PointerBehavior.Default, InputSourceType.Other, Handedness.Any); // this does not do what it should
           // PointerUtils.SetPointerBehavior<StylusPokePointer>(PointerBehavior.Default, InputSourceType.Other, Handedness.Any); 
            _stylusPokePointer.enabled = true;
            _stylusSpherePointer.enabled = true;
        }

        /// <summary>
        /// Disables the Cursor (Poke- and Grabpointers)
        /// </summary>
        private void DisableCursor()
        {
           // PointerUtils.SetPointerBehavior<StylusSpherePointer>(PointerBehavior.AlwaysOff, InputSourceType.Other, Handedness.Any);
           // PointerUtils.SetPointerBehavior<StylusPokePointer>(PointerBehavior.AlwaysOff, InputSourceType.Other, Handedness.Any);
            _stylusPokePointer.enabled = false;
            _stylusSpherePointer.enabled = false;
        }

        /// <summary>
        /// Enables the Ray (Raypointer)
        /// </summary>
        private void EnableRay()
        {
           // PointerUtils.SetPointerBehavior<StylusRayPointer>(PointerBehavior.Default, InputSourceType.Other, Handedness.Any);
            _stylusRayPointer.enabled = true;
        }

        /// <summary>
        /// Disables the Ray (Raypointer)
        /// </summary>
        private void DisableRay()
        {
           // PointerUtils.SetPointerBehavior<StylusRayPointer>(PointerBehavior.AlwaysOff, InputSourceType.Other, Handedness.Any);
            _stylusRayPointer.enabled = false;
        }

        void Update()
        {
            if (_initialized && _manager.CalibrationPreferences.StylusPreferredHand == StylusHoldingHand.Auto)
            {
                if (!_manager.IsPaired)
                {
                    EnableAllHandPointers(Handedness.Both);
                    return;
                }

                if (!_manager.DataParser.IsVisible)
                {
                    _stylusNotVisibleSureness++;
                }
                else
                {
                    _stylusNotVisibleSureness = _stylusNotVisibleSureness / 4;
                }

                if (_stylusNotVisibleSureness > 30)
                {
                    EnableAllHandPointers(Handedness.Both);
                    return;
                }

                Vector3 leftHandPosition = Vector3.zero;
                Vector3 rightHandPosition = Vector3.zero;
                Vector3 leftIndexTipPos = Vector3.zero;
                Vector3 rightIndexTipPos = Vector3.zero;
                Vector3 leftThumbTipPos = Vector3.zero;
                Vector3 rightThumbTipPos = Vector3.zero;

                bool leftHandFound = false;
                bool rightHandFound = false;
                bool leftIndexTipFound = false;
                bool rightIndexTipFound = false;
                bool leftThumbTipFound = false;
                bool rightThumbTipFound = false;

                foreach (var source in CoreServices.InputSystem.DetectedInputSources)
                {
                    // Ignore anything that is not a hand because we want articulated hands
                    if (source.SourceType == InputSourceType.Hand)
                    {
                        foreach (var p in source.Pointers)
                        {
                            var startPoint = p.Position;

                            if (p.Controller.ControllerHandedness == Handedness.Left)
                            {
                                leftHandPosition = startPoint;
                                leftHandFound = true;
                            }
                            else if (p.Controller.ControllerHandedness == Handedness.Right)
                            {
                                rightHandPosition = startPoint;
                                rightHandFound = true;
                            }


                            var hand = p.Controller as IMixedRealityHand;
                            if (hand != null)
                            {
                                // reading Index Tip Position
                                if (hand.TryGetJoint(TrackedHandJoint.IndexTip, out MixedRealityPose jointPose))
                                {
                                    if (hand.ControllerHandedness == Handedness.Left)
                                    {
                                        leftIndexTipFound = true;
                                        leftIndexTipPos = jointPose.Position;
                                    } else if (hand.ControllerHandedness == Handedness.Right)
                                    {
                                        rightIndexTipFound = true;
                                        rightIndexTipPos = jointPose.Position;
                                    }
                                }

                                // reading Thumg Tip Position
                                if (hand.TryGetJoint(TrackedHandJoint.ThumbTip, out MixedRealityPose thumbPose)) 
                                {
                                    if (hand.ControllerHandedness == Handedness.Left)
                                    {
                                        leftThumbTipFound = true;
                                        leftThumbTipPos = thumbPose.Position;
                                    }
                                    else if (hand.ControllerHandedness == Handedness.Right)
                                    {
                                        rightThumbTipFound = true;
                                        rightThumbTipPos = thumbPose.Position;
                                    }
                                }
                            }
                        }
                    }
                }

                if (rightHandFound == false)
                {
                    rightHandPosition = _stylusTransform.Position + new Vector3(2, 2, 2); /// when hand is not visible, just make a fake position which is very far away
                }

                if (leftHandFound == false)
                {
                    leftHandPosition = _stylusTransform.Position - new Vector3(-2, -2, -2); /// when hand is not visible, just make a fake position which is very far away
                }

                if (leftIndexTipFound)
                {
                    // is Left Index Tip near Stylus => if yes, then enable the rightHandPointers if it is already sure on leftHand
                    if (Vector3.Distance(leftIndexTipPos, _stylusTransform.Position) < Vector3.Distance(rightHandPosition, _stylusTransform.Position))
                    {
                        if (_stylusOnLeftHandSureness > 30)
                        {
                            StylusIsInLeftHand();
                            _stylusOnRightHandSureness = 0;
                        }
                        else
                        {
                            _stylusOnLeftHandSureness += 2;
                        }
                    }
                }

                if (leftThumbTipFound)
                {
                    if (Vector3.Distance(leftThumbTipPos, _stylusTransform.Position) < Vector3.Distance(rightHandPosition, _stylusTransform.Position))
                    {
                        if (_stylusOnLeftHandSureness > 30)
                        {
                            StylusIsInLeftHand();
                            _stylusOnRightHandSureness = 0;
                        }
                        else
                        {
                            _stylusOnLeftHandSureness += 2;
                        }
                    }
                }

                if (rightIndexTipFound)
                {
                    if (Vector3.Distance(rightIndexTipPos, _stylusTransform.Position) < Vector3.Distance(leftHandPosition, _stylusTransform.Position))
                    {
                        if (_stylusOnRightHandSureness > 30)
                        {
                            StylusIsInRightHand();
                            _stylusOnLeftHandSureness = 0;
                        }
                        else
                        {
                            _stylusOnRightHandSureness += 2;
                        }
                    }
                }

                if (rightThumbTipFound)
                {
                    if (Vector3.Distance(rightThumbTipPos, _stylusTransform.Position) < Vector3.Distance(leftHandPosition, _stylusTransform.Position))
                    {
                        if (_stylusOnRightHandSureness > 30)
                        {
                            StylusIsInRightHand();
                            _stylusOnLeftHandSureness = 0;
                        }
                        else
                        {
                            _stylusOnRightHandSureness += 2;
                        }
                    }
                }

                if (leftHandFound && rightHandFound)
                {
                    if (Vector3.Distance(leftHandPosition, _stylusTransform.Position) < Vector3.Distance(rightHandPosition, _stylusTransform.Position))
                    {
                        if (_stylusOnLeftHandSureness > 30)
                        {
                            StylusIsInLeftHand();
                            _stylusOnRightHandSureness = 0;
                        }
                        else
                        {
                            _stylusOnLeftHandSureness++;
                        }
                    }
                    else
                    {
                        if (_stylusOnRightHandSureness > 30)
                        {
                            StylusIsInRightHand();
                            _stylusOnLeftHandSureness = 0;
                        }
                        else
                        {
                            _stylusOnRightHandSureness++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Enables the left hand and disables the right hand
        /// This means, that the stylus is being hold in the right hand
        /// </summary>
        public void StylusIsInRightHand()
        {
            EnableAllHandPointers(Handedness.Left);

            DisableAllHandPointers(Handedness.Right);

            _stylusController.HoldingHand = StylusHoldingHand.Right;
        }

        /// <summary>
        /// Enables the right hand and disables the left hand
        /// This means, that the stylus is being hold in the left hand
        /// </summary>
        public void StylusIsInLeftHand()
        {
            EnableAllHandPointers(Handedness.Right);

            DisableAllHandPointers(Handedness.Left);

            _stylusController.HoldingHand = StylusHoldingHand.Left;
        }

        /// <summary>
        /// Enables all the hand pointers of the given handedness
        /// </summary>
        /// <param name="handedness"></param>
        private void EnableAllHandPointers(Handedness handedness)
        {
            PointerUtils.SetHandRayPointerBehavior(PointerBehavior.Default, handedness);
            PointerUtils.SetHandPokePointerBehavior(PointerBehavior.Default, handedness);
            PointerUtils.SetHandGrabPointerBehavior(PointerBehavior.Default, handedness);
        }

        /// <summary>
        /// Disables all the hand pointers of the given handedness
        /// </summary>
        private void DisableAllHandPointers(Handedness handedness)
        {
            PointerUtils.SetHandRayPointerBehavior(PointerBehavior.AlwaysOff, handedness);
            PointerUtils.SetHandPokePointerBehavior(PointerBehavior.AlwaysOff, handedness);
            PointerUtils.SetHandGrabPointerBehavior(PointerBehavior.AlwaysOff, handedness);
        }

        private void HandleHandPointers()
        {
            if (_stylusController == null) return;

            if (_stylusController.HoldingHand == StylusHoldingHand.Right)
            {
                StylusIsInRightHand();
            }
            else if (_stylusController.HoldingHand == StylusHoldingHand.Left)
            {
                StylusIsInLeftHand();
            }
        }

        private void OnStylusPreferedHandChanged(StylusData data)
        {
            HandleHandPointers();
        }

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            if (eventData.InputSource.SourceName.Contains("Stylus"))
            {
                Init();

                _manager.VisualSettings.Init(_manager);
            }
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
        }
    }
}