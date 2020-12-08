using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider))]
public class Seat : MonoBehaviour
{
    public enum SittingType
    { 
        Regular,
        Teleport,
    }

    [HideInInspector]
    public int id;

    public Transform sittingPoint;
    public Transform navigationPoint;

    public SittingType sittingType;

    public AnimationCurve SitDownAnim
    {
        get
        {
            switch (sittingType)
            {
                case SittingType.Regular:
                    return sitDownRegularAnim;
                case SittingType.Teleport:
                    return sitDownTeleportAnim;
                default:
                    return sitDownRegularAnim;
            }
        }
    }

    public AnimationCurve StandUpAnim
    {
        get
        {
            switch (sittingType)
            {
                case SittingType.Regular:
                    return standUpRegularAnim;
                case SittingType.Teleport:
                    return standUpTeleportAnim;
                default:
                    return standUpRegularAnim;
            }
        }
    }

    private static AnimationCurve sitDownRegularAnim = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0.5f), new Keyframe(1, 1, 0f, 0f));
    private static AnimationCurve standUpRegularAnim = new AnimationCurve(new Keyframe(0f, 0f, 0f, 2f), new Keyframe(1, 1, 0.5f, 0f));
    private static AnimationCurve sitDownTeleportAnim = new AnimationCurve(new Keyframe(0f, 1f));
    private static AnimationCurve standUpTeleportAnim = new AnimationCurve(new Keyframe(0f, 0f));

    public void OnEnable()
    {
        SeatManager.allSeats.Add(this);
        SeatManager.allSeatPositions.Add(navigationPoint.position);
    }

    private void OnDisable()
    {
        SeatManager.allSeats.Remove(this);
        SeatManager.allSeatPositions.Remove(navigationPoint.position);
    }

    private void Reset()
    {
        // Setup collider to child objects

        var col = GetComponent<BoxCollider>();
        Bounds bounds = new Bounds();

        foreach (var c in GetComponentsInChildren<MeshRenderer>())
        {
            if (bounds.size == Vector3.zero)
                bounds = new Bounds(c.bounds.center - transform.position, c.bounds.size);

            Bounds b = new Bounds(c.bounds.center - transform.position, c.bounds.size);
            bounds.Encapsulate(b);
        }

        col.center = bounds.center;
        col.size = bounds.size;

        col.isTrigger = true;

        // Try to find Sitting point and Navigation point
        foreach(var t in GetComponentsInChildren<Transform>())
        {
            if(t.gameObject.name.ToLower().Contains("nav"))
                navigationPoint = t;
            if (t.gameObject.name.ToLower().Contains("sit"))
                sittingPoint = t;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = UnityEditor.Selection.Contains(gameObject) ? Color.green : Color.white;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        Handles.DrawWireCube(GetComponent<BoxCollider>().center + transform.position, GetComponent<BoxCollider>().size);
    }
#endif
}
