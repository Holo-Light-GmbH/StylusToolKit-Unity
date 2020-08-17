using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HoloLight.STK.Core
{
    /// <summary>
    /// When a Panel needs to list some items and be able to navigate through them, then the Panel should extends this class
    /// </summary>
    public class ItemBrowser : MonoBehaviour
    {
        /// <summary>
        /// The next button to navigate through pages
        /// </summary>
        [SerializeField] protected GameObject _nextBtn;

        /// <summary>
        /// The previous button to navigate through pages
        /// </summary>
        [SerializeField] protected GameObject _previousBtn;

        /// <summary>
        /// The current page
        /// </summary>
        [SerializeField] protected TextMeshPro _pageText;

        /// <summary>
        /// The text that should be shown, when the list is empty
        /// </summary>
        [SerializeField] protected TextMeshPro _emptyText;

        /// <summary>
        /// The list of pages that can be navigated through
        /// </summary>
        protected List<GameObject> _pages = new List<GameObject>();

        /// <summary>
        /// The current open page; 0 = first page, 1 = second page, n = (n+1)-th page
        /// </summary>
        protected int _currentOpenPageIndex = 0;

        /// <summary>
        /// The parent gameobject, where the pages are created
        /// </summary>
        [SerializeField] protected GameObject _list;

        /// <summary>
        /// Mangages the previous and next button states. Depending on the number of pages, etc. they should be shown or not
        /// </summary>
        protected void HandlePageButtons()
        {
            if (_pages.Count >= 2)
            {
                _pageText.gameObject.SetActive(true);
                _pageText.text = _currentOpenPageIndex + 1 + " / " + _pages.Count;
                _nextBtn.SetActive(true);

                if (_currentOpenPageIndex + 1 == _pages.Count)
                {
                    _nextBtn.SetActive(false);
                }

                if (_currentOpenPageIndex >= 1)
                {
                    _previousBtn.SetActive(true);
                }

                if (_currentOpenPageIndex == 0)
                {
                    _previousBtn.SetActive(false);
                }
            }
            else
            {
                _pageText.gameObject.SetActive(false);
                _previousBtn.SetActive(false);
                _nextBtn.SetActive(false);
            }
        }

        /// <summary>
        /// When clicking on the next button, it shows the next page of files
        /// </summary>
        public void NextPage()
        {
            for (int i = 0; i < _pages.Count; i++)
            {
                _pages[i].SetActive(false);
            }

            _currentOpenPageIndex++;
            _pages[_currentOpenPageIndex].SetActive(true);

            HandlePageButtons();
        }

        /// <summary>
        /// When clicking on the previous button, it shows the previous page of files
        /// </summary>
        public void PreviousPage()
        {
            for (int i = 0; i < _pages.Count; i++)
            {
                _pages[i].SetActive(false);
            }

            _currentOpenPageIndex--;
            _pages[_currentOpenPageIndex].SetActive(true);
            HandlePageButtons();
        }
    }
}