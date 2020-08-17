using System;
using UnityEngine;

namespace HoloLight.STK.Examples.AllExamples
{
    /// <summary>
    /// Every Menu inside the UI has a unique MenuName
    /// These Names can be created here, and can be assigned to a menu in GUIManager Inspector
    /// </summary>
    public enum MenuName
    {
        Interactions = 0,
        Calibration = 1,
        Drawing = 2,
        Measurement = 3,
        Rotation = 4,
    }

    /// <summary>
    /// A Menu has a reference to its GameObject, has a Name an a bool to know if it is opened or not
    /// </summary>
    [Serializable]
    public class Menu
    {
        public GameObject GameObject;
        public MenuName Name;
        public bool IsOpen;

        public Menu(GameObject menuObject, MenuName name, bool isOpen = false)
        {
            this.GameObject = menuObject;
            this.Name = name;
            this.IsOpen = isOpen;
        }

        public void Open()
        {
            this.GameObject.SetActive(true);
            this.IsOpen = true;
        }

        public void Close()
        {
            this.GameObject.SetActive(false);
            this.IsOpen = false;
        }
    }
}
