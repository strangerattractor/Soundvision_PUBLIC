using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cylvester
{


    public class ListBox : MonoBehaviour
    {
        [SerializeField] GameObject itemTemplate;
        [SerializeField] GameObject content;
        [SerializeField] private StateManager stateManager;
        [SerializeField] ListBox listBox;

        public Dropdown.DropdownEvent onValueChanged;

        public List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

        private List<string> titleList;
        public void Start()
        {
            ClearOptions();

            //get all state names from stateManager into titleList
            titleList = stateManager.GetStateInfos().ToList();

            //listBox.AddOptions(buttons);
            listBox.AddOptions(titleList.Select(name => new Dropdown.OptionData(name)).ToList());
        }

        public void AddOptions(List<Dropdown.OptionData> optionData)
        {
            foreach (var option in optionData)
            { 
                //construct UI elements
            var copy = Instantiate(itemTemplate);
                copy.transform.SetParent(content.transform);
            //copy.transform.parent = content.transform;

            copy.GetComponentInChildren<TextMeshProUGUI>().text = option.text;

            int copyOfIndex = options.Count;
                //add event handler
            copy.GetComponent<Button>().onClick.AddListener(

                    () => {OnItemSelected(copyOfIndex);}
                );
                //add option to list
                options.Add(option);
            }
        }

        public void ClearOptions()
        {
            //remove UI components
            while (content.transform.childCount > 0)
            {
                Destroy(content.transform.GetChild(0));
            }
            //remove underlying data
            options.Clear();
        }

        private void OnItemSelected(int copyOfIndex)
        {
            print(copyOfIndex);
            stateManager.SelectedState = copyOfIndex;
        }

    }


}
