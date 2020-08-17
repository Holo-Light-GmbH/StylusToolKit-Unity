using UnityEngine;

namespace HoloLight.STK.Examples.Interactions
{
    /// <summary>
    /// Spawns a coffeecup 
    /// </summary>
    public class CoffeeSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject _coffeePrefab;

        public void SpawnNewCoffee()
        {
            GameObject coffee = Instantiate(_coffeePrefab, transform.parent);
            coffee.transform.position = transform.position;
        }
    }
}