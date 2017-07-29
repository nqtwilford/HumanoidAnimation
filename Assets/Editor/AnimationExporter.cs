using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationExporter : EditorWindow
{
    string mCurScenePath;
    GameObject[] mBodies;
    bool mSamplingCompleted;

    [MenuItem("Animation/Export #&m")]
    static void ShowWindow()
    {
        if (EditorApplication.isPlaying)
            return;
        GetWindow<AnimationExporter>(true);
    }

    void Awake()
    {
        ExportAll();
    }

    void OnGUI()
    {
        GUILayout.Label("采样过程中，请勿关闭此窗口，不要有任何动作！！！");
        if (GUILayout.Button("如果发生异常，点我手动退出采样"))
        {
            EditorSceneManager.OpenScene(mCurScenePath, OpenSceneMode.Single);
            Close();
        }
    }

    /*
    void OnEnable()
    {
        Debug.LogFormat("OnEnable, {0} {1} {2}",
            EditorApplication.isPlaying,
            EditorApplication.isPlayingOrWillChangePlaymode,
            mBodies);
    }

    void OnDisable()
    {
        Debug.LogFormat("OnDisable, {0} {1} {2}",
            EditorApplication.isPlaying,
            EditorApplication.isPlayingOrWillChangePlaymode,
            mBodies);
    }
    */

    void Update()
    {
        if (mBodies != null)
        {
            int completedCount = 0;
            foreach (GameObject body in mBodies)
            {
                if (body)
                {
                    var sampler = body.GetComponent<RuntimeSampleController>();
                    if (sampler.IsCompleted)
                        ++completedCount;
                }
            }
            if (completedCount == mBodies.Length)
                EndSampling();
        }
        if (mSamplingCompleted && !EditorApplication.isPlaying)
        {
            EditorSceneManager.OpenScene(mCurScenePath, OpenSceneMode.Single);
            Close();
            Debug.Log("采样完成！！！");
        }
    }

    void ExportAll()
    {
        mSamplingCompleted = false;
        mCurScenePath = SceneManager.GetActiveScene().path;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        mBodies = new GameObject[(int)BodyType.Count];
        try
        {
            // Static data exporting
            AnimatorControllerExporter.Export();

            // Static sampling
            AnimationMode.StartAnimationMode();
            var controller = CreateSampleAnimatorController();
            for (int i = 0; i < (int)BodyType.Count; ++i)
            {
                GameObject prefab = Resources.Load<GameObject>("FBX/Bodies/" + (BodyType)i);
                GameObject inst = Instantiate(prefab);
                inst.name = prefab.name;
                //inst.hideFlags = HideFlags.HideAndDontSave;
                inst.transform.position = Vector3.zero;
                inst.transform.forward = Vector3.forward;
                Animator animator = inst.GetComponent<Animator>();
                if (animator == null)
                    animator = inst.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = true;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                mBodies[i] = inst;
            }

            DribbleData dribbleData = DribbleSimSampler.Sample(controller.animationClips);
            //TargetMatchingSampler.Sample(mBodies, controller.animationClips);

            AnimationMode.StopAnimationMode();

            // Runtime sampling
            for (int i = 0; i < (int)BodyType.Count; ++i)
            {
                GameObject inst = mBodies[i];
                inst.AddComponent<RuntimeSampleController>();
                var sampler = inst.AddComponent<DribbleSimRuntimeSampler>();
                sampler.BodyType = (BodyType)i;
                sampler.Data = dribbleData;
                var sampler1 = inst.AddComponent<MovingSpeedRuntimeSampler>();
                sampler1.BodyType = (BodyType)i;
            }
            EditorApplication.isPlaying = true;
        }
        catch (System.Exception ex)
        {
            Debug.LogErrorFormat("ex:{0}", ex);
            for (int i = 0; i < (int)BodyType.Count; ++i)
            {
                GameObject inst = mBodies[i];
                if (inst != null)
                    DestroyImmediate(inst);
            }
            AnimationMode.StopAnimationMode();
            EditorSceneManager.OpenScene(mCurScenePath, OpenSceneMode.Single);
            Close();
        }
    }

    AnimatorController CreateSampleAnimatorController()
    {
#if DEBUG_SAMPLE
        AnimationClip[] clips = Resources.LoadAll<AnimationClip>("FBX/Animations/Char1@spinmove");
#else
        AnimationClip[] clips = Resources.LoadAll<AnimationClip>("FBX/Animations/");
#endif
        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(
            "Assets/Resources/Controllers/Sample.controller");
        ctrl.layers[0].stateMachine.AddState("None");
        foreach (AnimationClip clip in clips)
        {
            ctrl.AddMotion(clip);
        }
        return ctrl;
    }

    void EndSampling()
    {
        for (int i = 0; i < (int)BodyType.Count; ++i)
        {
            GameObject inst = mBodies[i];
            if (inst != null)
                DestroyImmediate(inst);
        }
        mBodies = null;
        AssetDatabase.SaveAssets();
        EditorApplication.isPlaying = false;
        mSamplingCompleted = true;
    }
}
