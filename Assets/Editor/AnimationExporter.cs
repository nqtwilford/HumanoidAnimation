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
        GameObject[] bodies = new GameObject[(int)BodyType.Count];
        try
        {
            var controller = Resources.Load<RuntimeAnimatorController>("Controllers/All");
            for (int i = 0; i < (int)BodyType.Count; ++i)
            {
                GameObject prefab = Resources.Load<GameObject>("FBX/Bodies/" + (BodyType)i);
                GameObject inst = Object.Instantiate(prefab);
                inst.name = prefab.name;
                inst.hideFlags = HideFlags.HideAndDontSave;
                inst.transform.position = Vector3.zero;
                Animator animator = inst.GetComponent<Animator>();
                if (animator == null)
                    animator = inst.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                bodies[i] = inst;
            }

            DribbleSimSampler.Sample(bodies, controller.animationClips);
            TargetMatchingSampler.Sample(bodies, controller.animationClips);
        }
        catch (System.Exception ex)
        {
            Debug.LogFormat("ex:{0}", ex);
        }
        finally
        {
            for (int i = 0; i < (int)BodyType.Count; ++i)
            {
                GameObject inst = bodies[i];
                if (inst != null)
                    Object.DestroyImmediate(inst);
            }
            AnimationMode.StopAnimationMode();
            AssetDatabase.SaveAssets();
        }

        //var ctrl = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>("Assets/Resources/FBX/Controllers/All.controller");
    }
}
