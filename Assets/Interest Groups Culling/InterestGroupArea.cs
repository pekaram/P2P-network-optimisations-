using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExitGames.Client.Photon;

using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Collider))]
public class InterestGroupArea : MonoBehaviour
{
    [Range(0,255)]
    [SerializeField]
    private int areaNumber;
    public int AreaNumber { get { return this.areaNumber; } }

    public event Action<InterestGroupArea> OnEnterArea;

    public event Action<InterestGroupArea> OnExitArea;

    private void OnCollisionEnter(Collision collision)
    {
        this.OnEnterArea?.Invoke(this);
    }

    private void OnCollisionExit(Collision collision)
    {
        this.OnExitArea?.Invoke(this);
    }
}
