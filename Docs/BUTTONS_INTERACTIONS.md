# Buttons & Interaction Components

<p align="center">
	<img src="imgs/Examples1.png" width="100%">
</p>

If you are not familar with the Interactable Component, first have a look here [Interactable Component](https://docs.microsoft.com/de-de/windows/mixed-reality/mrtk-unity/features/ux-building-blocks/interactable).

<p align="center">
	<img src="imgs/FarInteractionButton.png" width="40%">
</p>

## How to configure Interactable Component so it reacts to Stylus Button Interactions

Choose the `Select` from the Input Actions list so the interactable triggers on the (Front) Stylus Button. That's all :slightly_smiling_face: When you select the `Menu` it will trigger when you press the BACK Button.

<p align="center">
	<img src="imgs/InteractableStylusSelect.png" width="60%">
</p>

## How to configure Pressable Buttons to work by pressing it with the Stylus Tip 

<p align="center">
	<img src="imgs/PushableButton1.png" width="35%" style="display: inline">
	<img src="imgs/PushableButton2.png" width="35%" style="display: inline">
</p>

Good news! You don't have change anything. If the Input Action is set to `Select` of the Interactable Compontent where the PressableButton is attached to, it will react if the Stylus Tip presses that button. There you go :slightly_smiling_face:

A good reference for the pressable buttons is [MRTK Pressable Buttons](https://docs.microsoft.com/de-de/windows/mixed-reality/mrtk-unity/features/ux-building-blocks/button#collider-based-buttons-1)

## How to use Bounding Box and Manipulation Handler

<p align="center">
	<img src="imgs/BoundingBox1.png" width="70%">
</p>
Good news :slightly_smiling_face: You don’t have to change anything. 

Just have a look here to see how the MRTK Bounding Boxes work → [Bounding Box](https://docs.microsoft.com/de-de/windows/mixed-reality/mrtk-unity/features/ux-building-blocks/bounding-box)