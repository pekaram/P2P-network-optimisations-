using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class InputHandler : MonoBehaviour
{
    public event Action<Vector3> OnLeftMouseClicked;
    public LayerMask movementLayer;
    public LayerMask seatLayer;

    private Camera mainCamera;

    private void Start()
    {
        this.mainCamera = Camera.main;
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = this.mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit info;

            if (Physics.Raycast(ray, out info, float.PositiveInfinity, seatLayer))
            {
                OnLeftMouseClicked?.Invoke(info.transform.GetComponent<Seat>().navigationPoint.position);
            }
            else if (Physics.Raycast(ray, out info, float.PositiveInfinity, movementLayer))
            {
                OnLeftMouseClicked?.Invoke(info.point);
            }
        }
    }
}
