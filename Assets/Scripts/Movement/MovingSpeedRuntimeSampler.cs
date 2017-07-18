using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(RuntimeSampleController))]
public class MovingSpeedRuntimeSampler : MonoBehaviour, IRuntimeSampler
{
    static MovingData mData;

    public BodyType BodyType;

    MovingData.Clip mCurClipData;
    Vector3 mStartPosition;

    void Awake()
    {
        if (mData == null)
            mData = LoadData();
        GetComponent<RuntimeSampleController>().Samplers.Add(this);
    }

    static MovingData LoadData()
    {
        string filename = "Assets/Resources/Data/MovingSpeed.asset";
        MovingData data = AssetDatabase.LoadAssetAtPath<MovingData>(filename);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<MovingData>();
            AssetDatabase.CreateAsset(data, filename);
        }
        data.Clips.Clear();

        return data;
    }

    void IRuntimeSampler.OnStartClip(AnimationClip clip)
    {
        var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        //Debug.LogFormat("{0} loopTime:{1} keepOriginalXZ:{2} loopXZ:{3}", clip.name,
        //    clipSettings.loopTime, clipSettings.keepOriginalPositionXZ, clipSettings.loopBlendPositionXZ);
        // 循环播放的非原地动作，需要采样移动速度
        if (clipSettings.loopTime && !clipSettings.loopBlendPositionXZ)
        {
            int nameHash = Animator.StringToHash(clip.name);
            mCurClipData = mData.Clips.Find(c => c.NameHash == nameHash);
            if (mCurClipData == null)
            {
                mCurClipData = new MovingData.Clip()
                {
                    ClipName = clip.name,
                    NameHash = nameHash,
                };
                mData.Clips.Add(mCurClipData);
            }

            mStartPosition = transform.position;
        }
    }

    void IRuntimeSampler.OnFinishClip(AnimationClip clip)
    {
        if (mCurClipData != null)
        {
            Debug.Assert(clip.name == mCurClipData.ClipName);
            Vector3 endPosition = transform.position;
            mCurClipData.MovingSpeed[(int)BodyType] = (endPosition - mStartPosition).magnitude / clip.length;
            mCurClipData = null;
        }
    }

    void IRuntimeSampler.OnComplete()
    {
        EditorUtility.SetDirty(mData);
    }
}
