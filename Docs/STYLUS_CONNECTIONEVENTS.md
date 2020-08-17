# Stylus OnConnected/OnDisconnected

1. Add the Component StylusConnectionEvents to your GameObject
2. Reference the HoloStylusManager
3. Add your events inside OnStylusConnected and OnStylusDisconnected   

**OnStylusConnected** triggers when the application gets streaming data from the HMU<br>
**OnStylusDisconnected** triggers when the applicatoin doesn’t get anything from the HMU for 3s