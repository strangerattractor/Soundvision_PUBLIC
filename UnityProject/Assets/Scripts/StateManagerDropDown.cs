using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace cylvester
{
    public class StateManagerDropDown : MonoBehaviour
    {
        [SerializeField] private StateManager stateManager;
        private List <string> titleList;

        void Start () 
        {
            PopulateDropdown();
            Debug.Log("Titles are pulled");
        }

        private void PopulateDropdown()
        {
            Dropdown dropdown = GetComponent<Dropdown>();
            titleList = stateManager.GetStateInfos().ToList();
            dropdown.AddOptions(titleList);
        }

        public void Dropdown_IndexChanged(int index)
        {
            stateManager.SelectedState = index;
            Debug.Log(index);
        }
    }
}