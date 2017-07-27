using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MoveControl : MonoBehaviour
{
    const float TURN_SPEED = 0.08f;
    public const float MAX_SPEED = 2.8f;
    const float ACCELERATION = 20f;

    public float Speed
    {
        get { return mCurSpeed; }
        set
        {
            mCurSpeed = value;
            mAnimator.SetFloat("speed", mCurSpeed);
        }
    }
    float mCurSpeed;

    public bool InMovement
    {
        get { return mInMovement; }
        set
        {
            if (mInMovement != value)
            {
                mInMovement = value;
                //mPosition = transform.position;
                //if (value)
                //    Debug.Break();
            }
        }
    }
    bool mInMovement = true;

    Vector3 mVelocity;
    Vector3 mDirection = Vector3.forward;
    Vector3 mPosition;
    Transform mPosHolder;

    Animator mAnimator;

    void Awake()
    {
        mAnimator = GetComponent<Animator>();
        mPosition = transform.position;
        mPosHolder = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        mPosHolder.name = "Position Holder";
        mPosHolder.position = mPosition;
        mPosHolder.localScale = Vector3.one / 10;
        mPosHolder.GetComponent<Renderer>().material.color = Color.blue;
    }

    void Update()
    {
        if (InMovement)
        {
            Vector3 pos = mPosition;
            pos += mVelocity * Time.deltaTime;
            mPosition = pos;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f))
            {
                Speed = Mathf.Clamp(Speed - ACCELERATION * Time.deltaTime, 0f, MAX_SPEED);
                mVelocity = mDirection * Speed;
            }
            else
            {
                mDirection = new Vector3(h, 0, v).normalized;
                Speed = Mathf.Clamp(Speed + ACCELERATION * Time.deltaTime, 0f, MAX_SPEED);
                mVelocity = mDirection * Speed;
            }
        }
    }

    void LateUpdate()
    {
        if (InMovement)
        {
            transform.position = mPosition;
        }
        else
            mPosition = transform.position;
        transform.forward = Vector3.RotateTowards(transform.forward, mDirection, TURN_SPEED, 0f);
        mPosHolder.position = mPosition;
    }
}
