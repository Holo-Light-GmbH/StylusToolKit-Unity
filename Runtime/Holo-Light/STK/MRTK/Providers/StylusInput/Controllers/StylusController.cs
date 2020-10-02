using HoloLight.STK.Core;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using static HoloLight.STK.Core.CalibrationPreferences;

namespace HoloLight.STK.MRTK
{
    /// <summary>
    /// Implementation of Stylus Controller.
    /// </summary>
    [MixedRealityController(
        SupportedControllerType.Stylus,
        new[] { Handedness.Any },
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
            new MixedRealityInteractionMapping(0, "Stylus Pose", AxisType.SixDof, DeviceInputType.StylusPointer),
            new MixedRealityInteractionMapping(1, "Stylus Action", AxisType.Digital, DeviceInputType.Select),
            new MixedRealityInteractionMapping(2, "Stylus Back", AxisType.Digital, DeviceInputType.StylusBack),
        };

        private MixedRealityPose controllerPose = MixedRealityPose.ZeroIdentity;

        public StylusData StylusData;

        public override void SetupDefaultInteractions(Handedness controllerHandedness)
        {
            /// find the right InputActions and assign them to the Interactions...
            MixedRealityInteractionMapping[] defaultInteractions = new MixedRealityInteractionMapping[DefaultInteractions.Length];
            MixedRealityInputAction[] inputActions = CoreServices.InputSystem.InputSystemProfile.InputActionsProfile.InputActions;

            for (int i = 0; i < DefaultInteractions.Length; i++)
            {
                MixedRealityInputAction mixedRealityAction = MixedRealityInputAction.None;

                for (int j = 0; j < inputActions.Length; j++)
                {
                    if (inputActions[j].Description.Contains("Stylus"))
                    {
                        if (inputActions[j].Description.Contains("Pose") && DefaultInteractions[i].Description == "Stylus Pose")
                        {
                            mixedRealityAction = inputActions[j];
                            break;
                        }

                        if (inputActions[j].Description.Contains("Back") && DefaultInteractions[i].Description == "Stylus Back")
                        {
                            mixedRealityAction = inputActions[j];
                            break;
                        }
                    }


                    if (inputActions[j].Description.Contains("Select") && DefaultInteractions[i].Description == "Stylus Action")
                    {
                        mixedRealityAction = inputActions[j];
                        break;
                    }
                }

                MixedRealityInteractionMapping newInteraction = new MixedRealityInteractionMapping((uint)i, DefaultInteractions[i].Description, DefaultInteractions[i].AxisType, DefaultInteractions[i].InputType, mixedRealityAction);
                defaultInteractions[i] = newInteraction;
            }

            AssignControllerMappings(defaultInteractions);
        }


        private bool enabled = true;

        public StylusHoldingHand HoldingHand = StylusHoldingHand.Right;

        /// <summary>
        /// The position will not be updated anymore when disabling this. (e.g. needed for the Calibration)
        /// </summary>
        public void DisablePositionChanges()
        {
            enabled = false;
        }

        /// <summary>
        /// Enables updating the Positions
        /// </summary>
        public void EnablePositionChanges()
        {
            enabled = true;
        }

        /// <summary>
        /// Update controller.
        /// </summary>
        public void Update()
        {

            for (int i = 0; i < Interactions?.Length; i++)
            {
                if (Interactions[i].InputType == DeviceInputType.StylusPointer && enabled)
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
                    controllerPose = MixedRealityPose.ZeroIdentity;

                    controllerPose.Rotation = Quaternion.LookRotation(direction, Camera.main.transform.up);
                    controllerPose.Position = stylusPosition;

                    Interactions[i].PoseData = controllerPose;

                    if (Interactions[i].Changed)
                    {
                        CoreServices.InputSystem.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction, controllerPose);
                    }
                }

                if (Interactions[i].AxisType == AxisType.Digital)
                {
                    bool keyButton = false;
                    if (Interactions[i].InputType == DeviceInputType.Select)
                    {
                        keyButton = StylusData.Buttons[0]; // ACTION BUTTON
                    }

                    if (Interactions[i].InputType == DeviceInputType.StylusBack)
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