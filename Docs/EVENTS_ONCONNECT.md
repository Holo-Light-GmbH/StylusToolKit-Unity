# Stylus OnConnected/OnDisconnected

1. Add the Component **StylusConnectionEvents** to your GameObject
2. Reference the HoloStylusManager
3. Add your events inside OnStylusConnected and OnStylusDisconnected   

**OnStylusConnected** triggers when the application gets streaming data from the HMU<br>
**OnStylusDisconnected** triggers when the Bluetooth Device disconnects (beginning with v1.0.4)

**OnStylusDisconnected** triggers when the application doesn’t get anything from the HMU for 3s (until v1.0.3)

<p align="center">
	<img src="imgs/Event_OnConnect.png" width="50%">
</p>