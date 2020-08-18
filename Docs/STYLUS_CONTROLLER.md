# Using Stylus with other Controllers

In order to make the **Interactables** work with the standard Input Methods like, Gaze+Airtap, Handray, etc. in combination WITH the Stylus, then this guide will help you with it.

<p align="center">
	<img src="imgs/InputActionHandler.png" width="60%">
</p>

First we add the Component **InputActionHandler** to that GameObject, where the Interactable Component is attached to. Then we just need to select the InputAction to **Stylus Action**. Then add one event on each OnInputAction(Started/Ended) and reference the GameObject with the Interactable and choose the SetInputDown (for OnInputActionStarted) and SetInputUp (for OnInputEnded). 

In the Interactable Component, change the Input Actions to **Select**. 

> :information_source: You can find such an example in the following Scene: `FullExamples -> InteractionsContent -> SpawnCoffeeBtn`

Thats it! Now, the Button will work with Stylus and the other configured Input Controllers.
The result should look like this:

<p align="center">
	<img src="imgs/InteractableInputActionHandler.png" width="60%">
</p>
