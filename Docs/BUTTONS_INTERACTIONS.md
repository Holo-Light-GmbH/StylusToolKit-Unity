# Buttons & Interaction Components

<p align="center">
	<img src="imgs/Examples1.png" width="100%">
</p>

If you are not familar with the Interactable Component, first have a look here [Interactable Component](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/README_Interactable.html).

<p align="center">
	<img src="imgs/FarInteractionButton.png" width="40%">
</p>

## How to configure Interactable Component so it reacts to Stylus Button Interactions

Select the `Stylus Action` from the Input Actions list so it triggers ONLY on the ACTION (Front) Button. That's all :slightly_smiling_face: When you select the `Stylus Back` it will trigger when you press the BACK Button.

> :information_source: If you want it to trigger as well with e.g. Hand Gestures then have a look here ([Using Stylus with other Controllers](STYLUS_CONTROLLER.md))

<p align="center">
	<img src="imgs/InteractableStylusAction.png" width="70%">
</p>

## How to configure Pressable Buttons to work by pressing it with the Stylus Tip 

<p align="center">
	<img src="imgs/PushableButton1.png" width="35%" style="display: inline">
	<img src="imgs/PushableButton2.png" width="35%" style="display: inline">
</p>

A good reference for the pressable buttons is [MRTK Pressable Buttons](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/README_Button.html#collider-based-buttons-1)

Just change the Input Action to `Stylus Action` of the Interactable Compontent where the PressableButton is attached to. There you go :slightly_smiling_face:


<p align="center">
	<img src="imgs/PushableButton3.png" width="70%">
</p>

## How to use Bounding Box and Manipulation Handler

<p align="center">
	<img src="imgs/BoundingBox1.png" width="70%">
</p>
Good news :slightly_smiling_face: You don’t have to change anything. 

Just have a look here to see how the MRTK Bounding Boxes work → [Bounding Box](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/README_BoundingBox.html)