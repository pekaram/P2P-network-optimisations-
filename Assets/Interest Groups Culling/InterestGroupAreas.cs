using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InterestGroupAreas : MonoBehaviour
{
    private Dictionary<int, InterestGroupArea> groupNumberToArea = new Dictionary<int, InterestGroupArea>();

    public event Action<InterestGroupArea> OnAnyAreaEntered;

    public event Action<InterestGroupArea> OnAnyAreaExited;

    public void AddArea(InterestGroupArea area)
    {
        this.groupNumberToArea.Add(area.AreaNumber, area);

        area.OnEnterArea += OnEnterArea;
        area.OnExitArea += OnExitArea;
    }

    private void OnExitArea(InterestGroupArea area)
    {
        this.OnAnyAreaExited.Invoke(area);
    }

    private void OnEnterArea(InterestGroupArea area)
    {
        this.OnAnyAreaEntered.Invoke(area);
    }  
}
