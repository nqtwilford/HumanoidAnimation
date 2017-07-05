using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMirror : MonoBehaviour
{
	public bool MirrorBall;
	//Transform leftBall;
	//Transform rightBall;
	//GameObject goRightBall;
	public Transform BallNode;

    // Use this for initialization
    void Start()
    {
		//leftBall = transform.Find ("Hips/Pelvis/Spine1/Spine2/Spine3/LeftShoulder/LeftArm/LeftForeArm/LeftHand/Leftball");
		//rightBall = transform.Find ("Hips/Pelvis/Spine1/Spine2/Spine3/RightShoulder/RightArm/RightForeArm/RightHand/Rightball");
		//GameObject goLeftBall = transform.Find ("LballGeo").gameObject;
		//goLeftBall.SetActive (false);
		//goRightBall = transform.Find ("RballGeo").gameObject;
    }

    void LateUpdate()
    {
		if (MirrorBall) 
		{
			Vector3 position = BallNode.position;
			position.x = -position.x;
			BallNode.position = position;
		}
    }
}
