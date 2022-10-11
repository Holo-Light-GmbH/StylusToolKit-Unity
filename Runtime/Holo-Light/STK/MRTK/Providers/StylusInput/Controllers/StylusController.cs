using HoloLight.STK.Core;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Unity.Profiling;
using UnityEngine;
using static HoloLight.STK.Core.CalibrationPreferences;

namespace HoloLight.STK.MRTK
{
    /// <summary>
    /// Implementation of Stylus Controller.
    /// </summary>
    [MixedRealityController(
        SupportedControllerType.GenericUnity,
        new[] { Handedness.Both },
        "Packages/com.hololight.stylustoolkit/Runtime/Holo-Light/STK/MRTK/Providers/StylusInput/Textures/StylusController")]
    public class StylusController : BaseController
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public StylusController(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
            : base(trackingState, controllerHandedness, inputSource, interactions)
        {
        }

        /// <summary>
        /// The Stylus default interactions.
        /// </summary>
        /// <remarks>A single interaction mapping works for both left and right controllers.</remarks>
        public override MixedRealityInteractionMapping[] DefaultInteractions { get; } = new[]
        {
            new MixedRealityInteractionMapping(0, "Stylus Pose", AxisType.SixDof, DeviceInputType.SpatialPointer),
            new MixedRealityInteractionMapping(1, "Stylus Action", AxisType.Digital, DeviceInputType.Select),
            new MixedRealityInteractionMapping(2, "Stylus Back", AxisType.Digital, DeviceInputType.Menu),
        };

        private MixedRealityPose _controllerPose = MixedRealityPose.ZeroIdentity;

        public StylusData StylusData;

        private bool _enabled = true;

        /// <summary>
        /// The actual hand holding the stylus. Can be only StylusHoldingHand.Left or .Right.
        /// </summary>
        public StylusHoldingHand HoldingHand { get; internal set; } = StylusHoldingHand.Right;

        /// <summary>
        /// The position will not be updated anymore when disabling this. (e.g. needed for the Calibration)
        /// </summary>
        public void DisablePositionChanges()
        {
            _enabled = false;
        }

        /// <summary>
        /// Enables updating the Positions
        /// </summary>
        public void EnablePositionChanges()
        {
            _enabled = true;
        }

        private static readonly ProfilerMarker UpdatePerfMarker = new ProfilerMarker("[MRTK] StylusController.Update");

        /// <summary>
        /// Update controller.
        /// </summary>
        public void Update()
        {
            using (UpdatePerfMarker.Auto())
            {
                for (int i = 0; i < Interactions?.Length; i++)
                {
                    if (Interactions[i].InputType == DeviceInputType.SpatialPointer && _enabled)
                    {
                        Vector3 stylusPosition = StylusData.Position;

                        IsPositionAvailable = stylusPosition != Vector3.zero;

                        Vector3 fromPosition = Camera.main.transform.position;
                        Vector3 toPosition = stylusPosition;
                        Vector3 direction;

                        if (HoldingHand == StylusHoldingHand.Left)
                        {
                            direction = toPosition - fromPosition + Camera.main.transform.up * 0.06f + Camera.main.transform.right * 0.08f;
                        }
                        else
                        {
                            direction = toPosition - fromPosition + Camera.main.transform.up * 0.06f + Camera.main.transform.right * -0.08f;
                        }
                        // Stylus pointer raises Pose events  
                        _controllerPose = MixedRealityPose.ZeroIdentity;

                        _controllerPose.Rotation = Quaternion.LookRotation(direction, Camera.main.transform.up);
                        _controllerPose.Position = stylusPosition;

                        Interactions[i].PoseData = _controllerPose;

                        if (Interactions[i].Changed)
                        {
                            CoreServices.InputSystem.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction, _controllerPose);
                        }
                    }

                    if (Interactions[i].AxisType == AxisType.Digital)
                    {
                        bool keyButton = false;
                        if (Interactions[i].InputType == DeviceInputType.Select)
                        {
                            keyButton = StylusData.Buttons[0]; // ACTION BUTTON
                        }

                        if (Interactions[i].InputType == DeviceInputType.Menu)
                        {
                            keyButton = StylusData.Buttons[1]; // BACK BUTTON
                        }

                        // Update the interaction data source
                        Interactions[i].BoolData = keyButton;

                        // If our value changed raise it.
                        if (Interactions[i].Changed)
                        {
                            // Raise input system event if it's enabled
                            if (Interactions[i].BoolData)
                            {
                                CoreServices.InputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                            else
                            {
                                CoreServices.InputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                        }
                    }
                }
            }
        }
    }
}