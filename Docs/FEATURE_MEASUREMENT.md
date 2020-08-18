# Measurement

<p align="center">
	<img src="imgs/Feature_Measurement1.png" width="80%">
</p>

In order to use the Measurement Feature, you need the **MeasurementManager** prefab.(can be found in `Holo-Light/STK/Core/Features/Measurement/Prefabs/MeasurementManager.prefab`)

You just need to call the function Activate() to start with the measuring process. The first ACTION click will set the first point, the second click will set the second point. Then the line between these points will be drawn and the distance shown. That's all! :slightly_smiling_face:

**Example: Start the Measurement by pressing on a Button**

<p align="center">
	<img src="imgs/Feature_Measurement2.png" width="60%">
</p>

**Stop the Measurement by pressing the Stylus Back**

<p align="center">
	<img src="imgs/Feature_Measurement3.png" width="60%">
</p>

**Activate()**

Starts the measurement process

**Deactivate()**

Stops the measurement process

**GetMeasurements()**

Gets a List of Measurements that were created

**Undo()**

Removes last Measurement Line

**DeleteAll()**

Removes all Measurement Lines