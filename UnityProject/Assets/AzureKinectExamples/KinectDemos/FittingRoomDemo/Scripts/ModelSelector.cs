using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// ModelSelector controls the virtual model selection, as well as instantiates and sets up the selected model to overlay the user.
    /// </summary>
    public class ModelSelector : MonoBehaviour
    {
        [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
        public int playerIndex = 0;

        [Tooltip("The model category. Used for model discovery and title of the category menu.")]
        public string modelCategory = "Clothing";

        [Tooltip("Total number of the available clothing models.")]
        public int numberOfModels = 3;

        //	[Tooltip("Screen x-position of the model selection window. Negative values are considered relative to the screen width.")]
        //	public int windowScreenX = -160;

        [Tooltip("Reference to the dresing menu.")]
        public RectTransform dressingMenu;

        [Tooltip("Reference to the dresing menu-item prefab.")]
        public GameObject dressingItemPrefab;

        [Tooltip("Makes the initial model position relative to this camera, to be equal to the player's position, relative to the sensor.")]
        public Camera modelRelativeToCamera = null;

        [Tooltip("Camera used to estimate the overlay position of the model over the background.")]
        public Camera foregroundCamera;

        [Tooltip("Whether to keep the selected model, when the model category gets changed.")]
        public bool keepSelectedModel = true;

        [Tooltip("Whether the scale is updated continuously or just once, after the calibration pose.")]
        public bool continuousScaling = true;

        [Tooltip("Full body scale factor (incl. height, arms and legs) that might be used for fine tuning of body-scale.")]
        [Range(0.0f, 2.0f)]
        public float bodyScaleFactor = 1.0f;

        [Tooltip("Body width scale factor that might be used for fine tuning of the width scale. If set to 0, the body-scale factor will be used for the width, too.")]
        [Range(0.0f, 2.0f)]
        public float bodyWidthFactor = 1.0f;

        [Tooltip("Additional scale factor for arms that might be used for fine tuning of arm-scale.")]
        [Range(0.0f, 2.0f)]
        public float armScaleFactor = 1.0f;

        [Tooltip("Additional scale factor for legs that might be used for fine tuning of leg-scale.")]
        [Range(0.0f, 2.0f)]
        public float legScaleFactor = 1.0f;

        [Tooltip("Horizontal offset of the avatar with respect to the position of user's spine-base.")]
        [Range(-0.5f, 0.5f)]
        public float horizontalOffset = 0f;

        [Tooltip("Vertical offset of the avatar with respect to the position of user's spine-base.")]
        [Range(-0.5f, 0.5f)]
        public float verticalOffset = 0f;

        [Tooltip("Forward (Z) offset of the avatar with respect to the position of user's spine-base.")]
        [Range(-0.5f, 0.5f)]
        public float forwardOffset = 0f;

        [Tooltip("Whether to apply the humanoid model's muscle limits to the avatar, or not.")]
        private bool applyMuscleLimits = false;

        [Tooltip("Gender filter of this model selector.")]
        [HideInInspector]  // not currently supported
        public UserGender modelGender = UserGender.Unisex;
        public enum UserGender : int { Unisex = 0, Male = 1, Female = 2 };

        [Tooltip("Minimum age filter of this model selector.")]
        [HideInInspector]  // not currently supported
        public float minimumAge = 0;

        [Tooltip("Maximum age filter of this model selector.")]
        [HideInInspector]  // not currently supported
        public float maximumAge = 1000;


        [HideInInspector]
        public bool activeSelector = false;


        // Reference to the dresing menu list title
        private Text dressingMenuTitle;

        // Reference to the dresing menu list content
        private RectTransform dressingMenuContent;

        // list of instantiated dressing panels
        private List<GameObject> dressingPanels = new List<GameObject>();

        //private Rect menuWindowRectangle;
        private string[] modelNames;
        private Texture2D[] modelThumbs;

        private Vector2 scroll;
        private int selected = -1;
        private int prevSelected = -1;

        private GameObject selModel;

        private float curScaleFactor = 0f;
        private float curModelOffset = 0f;


        /// <summary>
        /// Sets the model selector to be active or inactive.
        /// </summary>
        /// <param name="bActive">If set to <c>true</c> b active.</param>
        public void SetActiveSelector(bool bActive)
        {
            activeSelector = bActive;

            if (dressingMenu)
            {
                dressingMenu.gameObject.SetActive(activeSelector);

                if (activeSelector)
                {
                    // update menu items
                    UpdateDressingMenu();
                }
            }

            if (!activeSelector && !keepSelectedModel)
            {
                // destroy currently selected model
                DestroySelectedModel();
            }
        }


        /// <summary>
        /// Gets the selected model.
        /// </summary>
        /// <returns>The selected model.</returns>
        public GameObject GetSelectedModel()
        {
            return selModel;
        }


        /// <summary>
        /// Destroys the currently selected model.
        /// </summary>
        public void DestroySelectedModel()
        {
            if (selModel)
            {
                AvatarController ac = selModel.GetComponent<AvatarController>();
                GameObject.Destroy(selModel);

                selModel = null;
                prevSelected = -1;
            }
        }


        /// <summary>
        /// Selects the next model.
        /// </summary>
        public void SelectNextModel()
        {
            selected++;
            if (selected >= numberOfModels)
                selected = 0;

            //LoadModel(modelNames[selected]);
            OnDressingItemSelected(selected);
        }


        /// <summary>
        /// Selects the previous model.
        /// </summary>
        public void SelectPrevModel()
        {
            selected--;
            if (selected < 0)
                selected = numberOfModels - 1;

            //LoadModel(modelNames[selected]);
            OnDressingItemSelected(selected);
        }


        /// <summary>
        /// Updates the dressing menu title & items.
        /// </summary>
        public void UpdateDressingMenu()
        {
            // get references to menu title and content
            if (!dressingMenuContent && dressingMenu)
            {
                Transform dressingHeaderText = dressingMenu.transform.Find("Header/Text");
                if (dressingHeaderText)
                {
                    dressingMenuTitle = dressingHeaderText.gameObject.GetComponent<Text>();
                }

                Transform dressingViewportContent = dressingMenu.transform.Find("Scroll View/Viewport/Content");
                if (dressingViewportContent)
                {
                    dressingMenuContent = dressingViewportContent.gameObject.GetComponent<RectTransform>();
                }
            }

            // create model names and thumbs
            modelNames = new string[numberOfModels];
            modelThumbs = new Texture2D[numberOfModels];
            dressingPanels.Clear();

            // remove current menu items
            dressingMenuContent.transform.DetachChildren();

            // instantiate menu items
            for (int i = 0; i < numberOfModels; i++)
            {
                modelNames[i] = string.Format("{0:0000}", i);

                string previewPath = modelCategory + "/" + modelNames[i] + "/preview.jpg";
                TextAsset resPreview = Resources.Load(previewPath, typeof(TextAsset)) as TextAsset;

                if (resPreview == null)
                {
                    resPreview = Resources.Load("nopreview.jpg", typeof(TextAsset)) as TextAsset;
                }

                //if(resPreview != null)
                {
                    modelThumbs[i] = CreatePreviewTexture(resPreview != null ? resPreview.bytes : null);
                }

                InstantiateDressingItem(i);
            }

            // select the 1st item
            if (numberOfModels > 0)
            {
                selected = 0;
            }

            // set the panel title
            if (dressingMenuTitle)
            {
                dressingMenuTitle.text = modelCategory;
            }
        }


        void Start()
        {
            // save current scale factors and model offsets
            curScaleFactor = bodyScaleFactor + bodyWidthFactor + armScaleFactor + legScaleFactor;
            curModelOffset = horizontalOffset + verticalOffset + forwardOffset + (applyMuscleLimits ? 1f : 0f);
        }

        void Update()
        {
            // check for selection change
            if (activeSelector && selected >= 0 && selected < modelNames.Length && prevSelected != selected)
            {
                KinectManager kinectManager = KinectManager.Instance;

                if (kinectManager && kinectManager.IsInitialized() && kinectManager.IsUserDetected(playerIndex))
                {
                    OnDressingItemSelected(selected);
                }
            }

            if (selModel != null)
            {
                // update model settings as needed
                float curMuscleLimits = applyMuscleLimits ? 1f : 0f;
                float updModelOffset = horizontalOffset + verticalOffset + forwardOffset + curMuscleLimits;

                if (Mathf.Abs(curModelOffset - updModelOffset) >= 0.001f)
                {
                    // update model offsets
                    curModelOffset = updModelOffset;

                    AvatarController ac = selModel.GetComponent<AvatarController>();
                    if (ac != null)
                    {
                        ac.horizontalOffset = horizontalOffset;
                        ac.verticalOffset = verticalOffset;
                        ac.forwardOffset = forwardOffset;
                        ac.applyMuscleLimits = applyMuscleLimits;
                    }
                }

                if (Mathf.Abs(curScaleFactor - (bodyScaleFactor + bodyWidthFactor + armScaleFactor + legScaleFactor)) >= 0.001f)
                {
                    // update scale factors
                    curScaleFactor = bodyScaleFactor + bodyWidthFactor + armScaleFactor + legScaleFactor;

                    AvatarScaler scaler = selModel.GetComponent<AvatarScaler>();
                    if (scaler != null)
                    {
                        scaler.continuousScaling = continuousScaling;
                        scaler.bodyScaleFactor = bodyScaleFactor;
                        scaler.bodyWidthFactor = bodyWidthFactor;
                        scaler.armScaleFactor = armScaleFactor;
                        scaler.legScaleFactor = legScaleFactor;
                    }
                }
            }
        }

        // creates preview texture
        private Texture2D CreatePreviewTexture(byte[] btImage)
        {
            Texture2D tex = new Texture2D(4, 4);
            //Texture2D tex = new Texture2D(100, 143);

            if (btImage != null)
            {
                tex.LoadImage(btImage);
            }

            return tex;
        }

        // instantiates dressing menu item
        private void InstantiateDressingItem(int i)
        {
            if (!dressingItemPrefab && i >= 0 && i < numberOfModels)
                return;
            if (!dressingMenuContent)
                return;

            GameObject dressingItemInstance = Instantiate<GameObject>(dressingItemPrefab);

            GameObject dressingImageObj = dressingItemInstance.transform.Find("DressingImagePanel").gameObject;
            dressingImageObj.GetComponentInChildren<RawImage>().texture = modelThumbs[i];

            if (!string.IsNullOrEmpty(modelNames[i]))
            {
                EventTrigger trigger = dressingItemInstance.GetComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();

                entry.eventID = EventTriggerType.Select;
                entry.callback.AddListener((eventData) => { OnDressingItemSelected(i); });

                trigger.triggers.Add(entry);
            }

            //if (dressingMenuContent) 
            {
                dressingItemInstance.transform.SetParent(dressingMenuContent, false);
            }

            dressingPanels.Add(dressingItemInstance);
        }

        // invoked when dressing menu-item was clicked
        private void OnDressingItemSelected(int i)
        {
            if (i >= 0 && i < modelNames.Length && prevSelected != i)
            {
                prevSelected = selected = i;
                LoadDressingModel(modelNames[selected]);
            }
        }

        // sets the selected dressing model as user avatar
        private void LoadDressingModel(string modelDir)
        {
            string modelPath = modelCategory + "/" + modelDir + "/model";
            UnityEngine.Object modelPrefab = Resources.Load(modelPath, typeof(GameObject));
            if (modelPrefab == null)
                return;

            Debug.Log("Model: " + modelPath);

            if (selModel != null)
            {
                GameObject.Destroy(selModel);
            }

            selModel = (GameObject)GameObject.Instantiate(modelPrefab, Vector3.zero, Quaternion.Euler(0, 180f, 0));
            selModel.name = "Model" + modelDir;

            AvatarController ac = selModel.GetComponent<AvatarController>();
            if (ac == null)
            {
                ac = selModel.AddComponent<AvatarController>();
                ac.playerIndex = playerIndex;

                ac.mirroredMovement = true;
                ac.verticalMovement = true;
                ac.applyMuscleLimits = applyMuscleLimits;

                ac.horizontalOffset = horizontalOffset;
                ac.verticalOffset = verticalOffset;
                ac.forwardOffset = forwardOffset;
                ac.smoothFactor = 0f;
            }

            ac.posRelativeToCamera = modelRelativeToCamera;
            ac.posRelOverlayColor = (foregroundCamera != null);
            ac.Update();

            KinectManager km = KinectManager.Instance;
            AvatarScaler scaler = selModel.GetComponent<AvatarScaler>();

            if (scaler == null)
            {
                scaler = selModel.AddComponent<AvatarScaler>();
                scaler.playerIndex = playerIndex;
                scaler.mirroredAvatar = true;
                scaler.minUserDistance = km.minUserDistance;

                scaler.continuousScaling = continuousScaling;
                scaler.bodyScaleFactor = bodyScaleFactor;
                scaler.bodyWidthFactor = bodyWidthFactor;
                scaler.armScaleFactor = armScaleFactor;
                scaler.legScaleFactor = legScaleFactor;
            }

            scaler.foregroundCamera = foregroundCamera;
            //scaler.debugText = debugText;
            scaler.Update();
        }

    }
}
