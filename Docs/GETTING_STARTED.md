# Prerequisites

To get started with the Stylus Toolkit (STK), you will need:

* Visual Studio 2019
* Unity 2018.4.x, Unity 2019 (MRTK supports both IL2CPP and .NET scripting backends on Unity 2018 (STK too))
* Windows SDK 18362+ (This is necessary if you are building a UWP app for WMR, HoloLens 1, or HoloLens 2)

# Step by Step Guide

1. Import MRTK into your Project (https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/GettingStartedWithTheMRTK.html) - if MRTK is not included yet
2. Import MRTK_FIX into your Project (STK_MRTK_2.3_FIX or STK_MRTK_2.4_FIX => these files can be found in the [release section](https://github.com/Holo-Light-GmbH/StylusToolKit-Unity/releases))
3. Import Stylus Toolkit (STK)
- Open in Unity → Window → Package Manager (2 different ways)
  - Add package from disk … (Download the Package and extract it on your hard drive and select the package.json file => the Package is attached at the bottom as .zip)
  - Add package from git URL … (https://github.com/Holo-Light-GmbH/StylusToolKit-Unity.git)

Recommended Step to get into STK:

4. Select the Stylus XR Toolkit Package and Import the Samples by clicking on “Import into Project”

5. Next Step: Setup Scene for StylusXR