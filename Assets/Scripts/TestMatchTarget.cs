using UnityEngine;
using System.Collections;

public class TestMatchTarget : MonoBehaviour
{
    public float x;
    Transform target;
    Animator animator;

    void Start()
    {
        transform.Find("BallGeo").gameObject.SetActive(false);
        transform.Find("BallGeoL").gameObject.SetActive(false);
        animator = GetComponent<Animator>();
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name + "_target";
        target = go.transform;
        target.localScale = Vector3.one / 10;
        //target.localPosition = transform.position + new Vector3(0f, 2.5f, 2f);
        target.localPosition = transform.position + new Vector3(-3f, 0f, 4.5f);
        StartCoroutine(DunkForever());
    }

    IEnumerator DunkForever()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            yield return Dunk();
        }
    }

    IEnumerator Dunk()
    {
        Reset();
        animator.Play("spinmove", 0, 0f);
        yield return null;
        animator.MatchTarget(target.position, target.rotation, AvatarTarget.Root,
            new MatchTargetWeightMask(new Vector3(1f, 0f, 1f), 0f), 0.169f, 1f);
            //new MatchTargetWeightMask(Vector3.one, 0f), 0.1f, 0.55f);
        yield return new WaitForSeconds(0.8f);
        Debug.Break();
    }

    void Reset()
    {
        transform.position = new Vector3(x, 0, 0);
    }
}
