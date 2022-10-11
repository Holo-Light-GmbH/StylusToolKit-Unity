using HoloLight.STK.Core;
using UnityEditor;
using UnityEngine;

namespace HoloLight.STK.Editor
{
	public class ConfigMenuHelper : ScriptableObject
	{
		[SerializeField]
		public GameObject _stylusPrefab; 
		/// <summary>
		/// Add menu item "Add RemoteCamera" to ISAR menu.
		/// </summary>
		[MenuItem("Holo-Light/Add Stylus Prefab")]
		static void ConfigureStylus()
		{
			AddStylusPrefab();
		}

		/// <summary>
		/// Add RemoteCamera script to main camera.
		/// </summary>
		private static void AddStylusPrefab()
		{
			Debug.Log("Holo-Light: Trying to add Stylus ...");

			HoloStylusManager manager = null;

			GameObject stylusGO = GameObject.Find("Stylus");

			if (stylusGO != null)
			{
				manager = stylusGO.GetComponent<HoloStylusManager>();
				if (manager != null)
				{
					Debug.Log("Holo-Light: Stylus already exists in the Scene");
				} else
				{
					Debug.Log("Holo-Light: Removed old Stylus GameObject which was not correctly configured");

					DestroyImmediate(stylusGO);
					AddPrefab();
				}
			} else if (GameObject.FindObjectOfType<HoloStylusManager>() == null)
			{
				AddPrefab();
			} else
            {
				Debug.Log("Holo-Light: Stylus already exists in the Scene");
			}
		}

		private static void AddPrefab()
        {
			Object prefab = AssetDatabase.LoadAssetAtPath("Packages/com.hololight.stylustoolkit/Runtime/Holo-Light/STK/Prefabs/Stylus.prefab", typeof(GameObject));

			GameObject mixedRealityPlayspace = GameObject.Find("MixedRealityPlayspace");
			if (mixedRealityPlayspace)
			{
				Debug.Log("Holo-Light: Successfully added Stylus as child of MixedRealityPlayspace");
				PrefabUtility.InstantiatePrefab(prefab, mixedRealityPlayspace.transform);
			}
			else
			{
				Debug.Log("Holo-Light: Successfully added Stylus to the Scene");
				PrefabUtility.InstantiatePrefab(prefab);
			}
		}
	}
}