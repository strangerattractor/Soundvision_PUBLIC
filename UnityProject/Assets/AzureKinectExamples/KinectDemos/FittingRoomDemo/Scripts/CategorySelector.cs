using UnityEngine;
using System.Collections;
using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    public class CategorySelector : MonoBehaviour, GestureListenerInterface
    {
        [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
        public int playerIndex = 0;

        [Tooltip("Whether to use swipe-left and swipe-right gestures to change the clothing model in the active category.")]
        public bool swipeToChangeModel = true;

        [Tooltip("Whether to use left and right hand-raise gestures to change the model category.")]
        public bool raiseHandToChangeCategory = true;

        [Tooltip("GUI-Text used to display information messages.")]
        public UnityEngine.UI.Text infoText;


        // available model selectors
        private ModelSelector[] allModelSelectors;
        private int iCurSelector = -1;

        // current model selector
        private ModelSelector modelSelector;

        // last detected userId;
        private ulong lastUserId = 0;


        /// <summary>
        /// Gets the active model selector.
        /// </summary>
        /// <returns>The active model selector.</returns>
        public ModelSelector GetActiveModelSelector()
        {
            return modelSelector;
        }


        /// <summary>
        /// Activate the next model selector.
        /// </summary>
        public void ActivateNextModelSelector()
        {
            if (allModelSelectors.Length > 0)
            {
                if (modelSelector)
                    modelSelector.SetActiveSelector(false);

                iCurSelector++;
                if (iCurSelector >= allModelSelectors.Length)
                    iCurSelector = 0;

                modelSelector = allModelSelectors[iCurSelector];
                modelSelector.SetActiveSelector(true);

                Debug.Log("Category: " + modelSelector.modelCategory);
            }
        }


        /// <summary>
        /// Activates the previous model selector.
        /// </summary>
        public void ActivatePrevModelSelector()
        {
            if (allModelSelectors.Length > 0)
            {
                if (modelSelector)
                    modelSelector.SetActiveSelector(false);

                iCurSelector--;
                if (iCurSelector < 0)
                    iCurSelector = allModelSelectors.Length - 1;

                modelSelector = allModelSelectors[iCurSelector];
                modelSelector.SetActiveSelector(true);

                Debug.Log("Category: " + modelSelector.modelCategory);
            }
        }


        /// <summary>
        /// Refreshes the list of available model selectors.
        /// </summary>
        public void RefreshModelSelectorsList(ModelSelector.UserGender gender, float age, bool bSelectFirst)
        {
            if (allModelSelectors != null && allModelSelectors.Length > 0)
            {
                if (modelSelector)
                    modelSelector.SetActiveSelector(false);
            }

            // find mono scripts containing model selectors
            ModelSelector[] monoScripts = GetComponents<ModelSelector>();

            int countEnabled = 0;
            if (monoScripts != null && monoScripts.Length > 0)
            {
                foreach (ModelSelector monoScript in monoScripts)
                {
                    ModelSelector modelSel = (ModelSelector)monoScript;

                    bool genderMatch = gender == ModelSelector.UserGender.Unisex || modelSel.modelGender == ModelSelector.UserGender.Unisex || modelSel.modelGender == gender;
                    bool ageMatch = age < 0 || (age >= modelSel.minimumAge && age <= modelSel.maximumAge);

                    if (modelSel.playerIndex == playerIndex && genderMatch && ageMatch)
                        countEnabled++;
                }
            }

            allModelSelectors = new ModelSelector[countEnabled];

            if (countEnabled > 0)
            {
                int j = 0;

                foreach (ModelSelector monoScript in monoScripts)
                {
                    ModelSelector modelSel = (ModelSelector)monoScript;

                    bool genderMatch = gender == ModelSelector.UserGender.Unisex || modelSel.modelGender == ModelSelector.UserGender.Unisex || modelSel.modelGender == gender;
                    bool ageMatch = age < 0 || (age >= modelSel.minimumAge && age <= modelSel.maximumAge);

                    if (modelSel.playerIndex == playerIndex && genderMatch && ageMatch)
                    {
                        allModelSelectors[j] = modelSel;
                        modelSel.SetActiveSelector(false);

                        j++;
                    }
                }
            }

            if (allModelSelectors.Length > 0 && bSelectFirst)
            {
                iCurSelector = 0;

                modelSelector = allModelSelectors[iCurSelector];
                modelSelector.SetActiveSelector(true);

                Debug.Log("Category: " + modelSelector.modelCategory);
            }

        }


        /////////////////////////////////////////////////////////////////////////////////


        void Start()
        {
            // create the initial model selectors list
            RefreshModelSelectorsList(ModelSelector.UserGender.Unisex, -1f, true);

            // check for KM and hint for calibration pose
            KinectManager manager = KinectManager.Instance;
            if (manager && manager.IsInitialized())
            {
                if (infoText != null && manager.playerCalibrationPose == GestureType.Tpose)
                {
                    infoText.text = "Please stand in T-pose for calibration.";
                }
            }
            else
            {
                string sMessage = "KinectManager is missing or not initialized";
                Debug.LogError(sMessage);

                if (infoText != null)
                {
                    infoText.text = sMessage;
                }
            }
        }


        void Update()
        {
            KinectManager manager = KinectManager.Instance;

            if (manager && manager.IsInitialized())
            {
                ulong userId = manager.GetUserIdByIndex(playerIndex);

                if (userId != 0 && lastUserId != userId)
                {
                    if (infoText != null)
                    {
                        string sMessage = swipeToChangeModel && modelSelector ? "Swipe left or right to change clothing." : string.Empty;
                        if (raiseHandToChangeCategory && allModelSelectors.Length > 1)
                            sMessage += " Raise hand to change category.";

                        infoText.text = sMessage;
                    }

                    lastUserId = userId;
                }

                if (userId == 0 && userId != lastUserId)
                {
                    lastUserId = userId;

                    // destroy currently loaded models
                    foreach (ModelSelector modSelector in allModelSelectors)
                    {
                        modSelector.DestroySelectedModel();
                    }

                    if (infoText != null && manager.playerCalibrationPose == GestureType.Tpose)
                    {
                        infoText.text = "Please stand in T-pose for calibration.";
                    }
                }
            }
        }


        public void UserDetected(ulong userId, int userIndex)
        {
            KinectManager kinectManager = KinectManager.Instance;
            KinectGestureManager gestureManager = kinectManager ? kinectManager.gestureManager : null;
            if (!gestureManager || (userIndex != playerIndex))
                return;

            if (raiseHandToChangeCategory)
            {
                gestureManager.DetectGesture(userId, GestureType.RaiseRightHand);
                gestureManager.DetectGesture(userId, GestureType.RaiseLeftHand);
            }

            if (swipeToChangeModel)
            {
                gestureManager.DetectGesture(userId, GestureType.SwipeLeft);
                gestureManager.DetectGesture(userId, GestureType.SwipeRight);
            }
        }

        public void UserLost(ulong userId, int userIndex)
        {
            if (userIndex != playerIndex)
                return;
        }

        public void GestureInProgress(ulong userId, int userIndex, GestureType gesture, float progress, KinectInterop.JointType joint, Vector3 screenPos)
        {
            // nothing to do here
        }

        public bool GestureCompleted(ulong userId, int userIndex, GestureType gesture, KinectInterop.JointType joint, Vector3 screenPos)
        {
            if (userIndex != playerIndex)
                return false;

            switch (gesture)
            {
                case GestureType.RaiseRightHand:
                    ActivateNextModelSelector();
                    break;
                case GestureType.RaiseLeftHand:
                    ActivatePrevModelSelector();
                    break;
                case GestureType.SwipeLeft:
                    if (modelSelector)
                    {
                        modelSelector.SelectNextModel();
                    }
                    break;
                case GestureType.SwipeRight:
                    if (modelSelector)
                    {
                        modelSelector.SelectPrevModel();
                    }
                    break;
            }

            return true;
        }

        public bool GestureCancelled(ulong userId, int userIndex, GestureType gesture, KinectInterop.JointType joint)
        {
            if (userIndex != playerIndex)
                return false;

            return true;
        }

    }
}
