using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using com.rfilkov.kinect;


namespace com.rfilkov.components
{
    /// <summary>
    /// MocapRecorder records the avatar motion into the given animation clip.
    /// </summary>
    public class MocapRecorder : MonoBehaviour
    {
        [Tooltip("The avatar, whose motion will be captured in the animation clip.")]
        public AvatarController avatarModel;

        [Tooltip("Full path to the file, where the animation clip will be saved at the end of animation recording.")]
        public string animSaveToFile = "Assets/AzureKinectExamples/KinectDemos/MocapAnimatorDemo/Animations/Recorded.anim";

        [Tooltip("Whether to capture the root motion as well.")]
        public bool captureRootMotion = true;

        [Tooltip("The model used to play the recorded animation clip.")]
        public MocapPlayer mocapPlayer;

        [Tooltip("Array of sprite transforms that will be used for displaying the countdown recording starts.")]
        public Transform[] countdown;

        [Tooltip("Sprite transform that displays recording-in-progress.")]
        public Transform recIcon;

        [Tooltip("UI Text to display information messages.")]
        public UnityEngine.UI.Text infoText;


        // reference to the avatar's animator component
        private Animator modelAnimator = null;

        // recording parameters
        private bool isRecording = false;
        private bool isCountingDown = false;
        private float animTime = 0;

        // human pose parameters
        private HumanPose humanPose = new HumanPose();
        private HumanPoseHandler humanPoseHandler = null;

        // animation curves to hold the recorded animation 
        private Dictionary<int, AnimationCurve> muscleCurves = new Dictionary<int, AnimationCurve>();
        private Dictionary<string, AnimationCurve> rootPoseCurves = new Dictionary<string, AnimationCurve>();

        // initial model's root position
        private Vector3 initialRootPos = Vector3.zero;



        void Start()
        {
            if(avatarModel)
            {
                modelAnimator = avatarModel.gameObject.GetComponent<Animator>();

                if (modelAnimator)
                {
                    initialRootPos = avatarModel.transform.position;
                    humanPoseHandler = new HumanPoseHandler(modelAnimator.avatar, avatarModel.transform);
                }
                else
                {
                    ShowMessage("The AvatarModel has no Animator-component!");
                }
            }
            else
            {
                ShowMessage("The AvatarModel is not set!");
            }
        }


        void Update()
        {
            // check for Space-key
            if(Input.GetButtonDown("Jump"))
            {
                if(!isRecording)
                {
                    if(!isCountingDown && avatarModel && avatarModel.playerId != 0)
                    {
                        InitAnimationCurves();
                        isCountingDown = true;
                        StartCoroutine(CountdownAndStartRecording());
                    }
                }
                else
                {
                    StopRecording();
                }
            }

            if (isRecording && avatarModel && avatarModel.playerId != 0)
            {
                // record the current pose
                animTime += Time.deltaTime;
                RecordAvatarPose();

                if (infoText & ((int)(animTime * 10f) % 10) == 0)
                {
                    infoText.text = string.Format("Recording... {0:F0}s", animTime);
                }
            }

            // stop recording, if the user is lost
            if (avatarModel && avatarModel.playerId == 0)
            {
                StopRecording();
            }
        }


        //void LateUpdate()
        //{
        //}


        // displays the given message on screen and logs it to console
        private void ShowMessage(string sMessage)
        {
            if (infoText)
            {
                infoText.text = sMessage;
            }

            Debug.Log(sMessage);
        }


        // counts down (from 3 for instance), then starts the animation recording
        private IEnumerator CountdownAndStartRecording()
        {
            if (countdown != null && countdown.Length > 0)
            {
                for (int i = 0; i < countdown.Length; i++)
                {
                    if (countdown[i])
                        countdown[i].gameObject.SetActive(true);

                    yield return new WaitForSeconds(1f);

                    if (countdown[i])
                        countdown[i].gameObject.SetActive(false);
                }
            }

            isCountingDown = false;
            isRecording = true;
            ShowMessage("Recording started.");

            if (recIcon)
            {
                recIcon.gameObject.SetActive(true);
            }
        }


        // clears the animation curves before the recording starts
        private void InitAnimationCurves()
        {
            if (avatarModel == null)
                return;

            animTime = 0f;
            muscleCurves.Clear();
            rootPoseCurves.Clear();

            List<HumanBodyBones> mecanimBones = avatarModel.GetMecanimBones();
            foreach (HumanBodyBones boneType in mecanimBones)
            {
                for (int i = 0; i < 3; i++)
                {
                    int muscle = HumanTrait.MuscleFromBone((int)boneType, i);

                    if (muscle != -1)
                    {
                        muscleCurves.Add(muscle, new AnimationCurve());
                    }
                }
            }

            rootPoseCurves.Add("RootT.x", new AnimationCurve());
            rootPoseCurves.Add("RootT.y", new AnimationCurve());
            rootPoseCurves.Add("RootT.z", new AnimationCurve());

            rootPoseCurves.Add("RootQ.x", new AnimationCurve());
            rootPoseCurves.Add("RootQ.y", new AnimationCurve());
            rootPoseCurves.Add("RootQ.z", new AnimationCurve());
            rootPoseCurves.Add("RootQ.w", new AnimationCurve());
        }


        // stops the recording and saves the animation clip
        private void StopRecording()
        {
            if(isRecording)
            {
                isRecording = false;
                ShowMessage(string.Format("Recording stopped - saved {0:F3}s animation.", animTime));

                if (recIcon)
                {
                    recIcon.gameObject.SetActive(false);
                }

                bool isAnythingRecorded = (muscleCurves.Count > 0 && muscleCurves[0].length > 0) || 
                    (rootPoseCurves.Count > 0 && rootPoseCurves["RootT.x"].length > 0);

                if (isAnythingRecorded)
                {
                    AnimationClip animClip = CreateAnimationClip();
                    SaveAnimationClip(animClip);

                    if (mocapPlayer)
                    {
                        mocapPlayer.PlayAnimationClip(animClip);
                    }
                }
                else
                {
                    ShowMessage("Recording stopped - nothing to save.");
                }
            }
        }


        // records the current avatar pose to the animation curves
        private void RecordAvatarPose()
        {
            humanPoseHandler.GetHumanPose(ref humanPose);

            foreach (KeyValuePair<int, AnimationCurve> data in muscleCurves)
            {
                Keyframe key = new Keyframe(animTime, humanPose.muscles[data.Key]);
                data.Value.AddKey(key);
            }

            if(captureRootMotion)
            {
                Vector3 rootPosition = humanPose.bodyPosition - initialRootPos;
                AddRootPosKeyFrame("RootT.x", rootPosition.x);
                AddRootPosKeyFrame("RootT.y", rootPosition.y);
                AddRootPosKeyFrame("RootT.z", rootPosition.z);

                Quaternion rootRotation = humanPose.bodyRotation;
                AddRootPosKeyFrame("RootQ.x", rootRotation.x);
                AddRootPosKeyFrame("RootQ.y", rootRotation.y);
                AddRootPosKeyFrame("RootQ.z", rootRotation.z);
                AddRootPosKeyFrame("RootQ.w", rootRotation.w);
            }
        }


        // adds a key frame for the given root position coordinate
        private void AddRootPosKeyFrame(string property, float value)
        {
            Keyframe key = new Keyframe(animTime, value);
            rootPoseCurves[property].AddKey(key);
        }


        // creates animation clip out of the recorded animation curves
        private AnimationClip CreateAnimationClip()
        {
            AnimationClip animClip = new AnimationClip();

            foreach (KeyValuePair<int, AnimationCurve> data in muscleCurves)
            {
                animClip.SetCurve(string.Empty, typeof(Animator), HumanTrait.MuscleName[data.Key], data.Value);
            }

            if(captureRootMotion)
            {
                foreach (KeyValuePair<string, AnimationCurve> data in rootPoseCurves)
                {
                    animClip.SetCurve(string.Empty, typeof(Animator), data.Key, data.Value);
                }

            }

            return animClip;
        }


        // saves the animation clip to the specified save-file
        private void SaveAnimationClip(AnimationClip animClip)
        {
            if(string.IsNullOrEmpty(animSaveToFile))
            {
                ShowMessage("Animation save path not set!");
                return;
            }

            // save the clip
            int iP = animSaveToFile.LastIndexOf('/');
            string animName = (iP >= 0 ? animSaveToFile.Substring(iP + 1) : animSaveToFile).Trim();

            if (animName.EndsWith(".anim"))
                animName = animName.Substring(0, animName.Length - 5);

            animClip.name = animName;

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(animClip, animSaveToFile);
            Debug.Log("Animation clip saved: " + animSaveToFile);
#else
            ShowMessage("The animation clip can be saved only in Unity editor.");
#endif

            // clear the animation curves
            muscleCurves.Clear();
            rootPoseCurves.Clear();
        }

    }
}
