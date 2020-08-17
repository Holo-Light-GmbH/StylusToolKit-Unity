using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HoloLight.STK.Examples.AllExamples
{
    /// <summary>
    /// Handles opening and closing, navigating the panels/menus. 
    /// </summary>
    public class GUIManager : MonoBehaviour
    {
        private static GUIManager _instance;

        public static GUIManager Instance
        {
            get { return _instance; }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        /// <summary>
        /// The menu list that can be navigated through.
        /// Change the count in inspector to add more menus and add a new menuName in the Menu.cs file
        /// </summary>
        [SerializeField] private List<Menu> _menus = new List<Menu>();

        /// <summary>
        /// The List of the menus you have navigated through. Used for the NavigateBack function
        /// </summary>
        private List<MenuName> _lastMenus;

        /// <summary>
        /// The current opened menu
        /// </summary>
        private Menu _currentOpenMenu;

        private int _currentIndex = 0;

        [SerializeField]
        private TextMeshPro _currentMenuName;
        void Start()
        {
            _lastMenus = new List<MenuName>();
            foreach (Menu menu in _menus)
            {
                if (menu.IsOpen)
                {
                    _currentOpenMenu = menu;
                } else
                {
                    menu.Close();
                }

            }

            if (_currentOpenMenu != null)
            {
                _currentOpenMenu.Open();
            }
        }

        /// <summary>
        /// Closes the current menu and opens the menu with the specified name
        /// </summary>
        /// <param name="menuName">The name of the menu to navigate to</param>
        /// <param name="isNavigatingBack">When it is not navigating back, it should remember the last menus, to be able to navigate back</param>
        public void NavigateTo(MenuName menuName, bool isNavigatingBack = false)
        {
            if (!isNavigatingBack)
            {
                Menu previousMenu = _currentOpenMenu;
                _lastMenus.Add(previousMenu.Name);
            }

            _currentOpenMenu.Close();

            foreach (Menu menu in _menus)
            {
                if (menu.Name == menuName)
                {
                    _currentOpenMenu = menu;
                }
            }
            _currentMenuName.text = _currentOpenMenu.Name.ToString();
            _currentOpenMenu.Open();
        }

        /// <summary>
        /// Used to call the NavigateTo function in the inspector, and set the Menu with the number
        /// (because enum types can not be selected as function parameters in Unity)
        /// </summary>
        /// <param name="menuType"></param>
        public void NavigateToInt(int menuType)
        {
            NavigateTo((MenuName) menuType);
        }

        /// <summary>
        /// Closes the currentOpen menu
        /// </summary>
        public void CloseMenu()
        {
            _currentOpenMenu.Close();
        }

        /// <summary>
        /// Navigates to the last menu it came from (Mainly the "BACK" button triggers this function)
        /// </summary>
        public void NavigateBack()
        {
            NavigateTo(_lastMenus[_lastMenus.Count - 1], true);
            _lastMenus.RemoveAt(_lastMenus.Count - 1);
        }

        public Menu GetCurrentOpenMenu()
        {
            return _currentOpenMenu;
        }

        public void NextContent()
        {
            _currentIndex = (int)_currentOpenMenu.Name;
            _currentIndex++; 
            if (_currentIndex >= _menus.Count)
            {
                _currentIndex = 0;
            }
            NavigateToInt(_currentIndex);
        }

        public void PreviousContent()
        {
            _currentIndex = (int)_currentOpenMenu.Name;
            _currentIndex--;
            if (_currentIndex < 0)
            {
                _currentIndex = _menus.Count-1;
            }
            NavigateToInt(_currentIndex);
        }

        public void CloseApplication()
        {
            Application.Quit();
        }
    }
}