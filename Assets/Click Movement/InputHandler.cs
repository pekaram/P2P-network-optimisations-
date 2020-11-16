using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class InputHandler : MonoBehaviour
{
    [SerializeField]
    private Transform movementPlane;

    public event Action<Vector3> OnLeftMouseClicked;

    private Camera mainCamera;

    private void Start()
    {
        this.mainCamera = Camera.main;
    }

    public void FixedUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Plane plane = new Plane(Vector3.up, movementPlane.position);
            Ray ray = this.mainCamera.ScreenPointToRay(Input.mousePosition);
            float point = 0f;

            if (plane.Raycast(ray, out point))
            {
                OnLeftMouseClicked?.Invoke(ray.GetPoint(point));
            }
        }
    }
}
