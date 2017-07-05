using UnityEngine;
using UnityEditor;

public class AnimationExporter// : AssetPostprocessor
{
    /*
    void OnPostprocessModel(GameObject go)
    {
        Debug.LogFormat("OnPostprocessModel: {0}", go.name);
        ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        foreach (var clip in importer.clipAnimations)
        {
            Debug.LogFormat("AnimationClip: {0}", clip.name);
            foreach (var evt in clip.events)
            {
                Debug.LogFormat("evt: {0} {1} {2}", evt.functionName, evt.stringParameter, evt.time);
            }
        }
    }
    */

    [MenuItem("Animation/Export")]
    static void ExportAll()
    {
        AnimationMode.StartAnimationMode();
        var controller = Resources.Load<RuntimeAnimatorController>("Controllers/All");
        GameObject[] models = Resources.LoadAll<GameObject>("FBX/Bodies");
        //GameObject[] bodies = new GameObject[models.Length];
        for (int i = 0; i < models.Length; ++i)
        {
            GameObject inst = Object.Instantiate(models[i]);
            inst.name = models[i].name;
            inst.hideFlags = HideFlags.HideAndDontSave;
            inst.transform.position = Vector3.zero;
            Animator animator = inst.GetComponent<Animator>();
            if (animator == null)
                animator = inst.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            DribbleSimSampler.Sample(inst, controller.animationClips);
            TargetMatchingSampler.Sample(inst, controller.animationClips);

            Object.DestroyImmediate(inst);
        }
        AnimationMode.StopAnimationMode();

        //var ctrl = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>("Assets/Resources/FBX/Controllers/All.controller");

        AssetDatabase.SaveAssets();
    }
}
