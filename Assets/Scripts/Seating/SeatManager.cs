using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SeatManager : MonoBehaviour
{
    public static List<Seat> allSeats = new List<Seat>();
    public static HashSet<Vector3> allSeatPositions = new HashSet<Vector3>();

    private void Start()
    {
        for (int i = 0; i < allSeats.Count; ++i)
            allSeats[i].id = i;
    }

    public static bool OccupySeat(Seat seat, int user)
    {
        if (!allSeats.Contains(seat))
            return false;

        RefreshSeatStates();

        var currentProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        // Add the seat to the properties if it has not been interacted with yet
        if (!currentProperties.ContainsKey(seat.id.ToString()))
            currentProperties.Add(seat.id.ToString(), -1);

        // Cannot sit in if the seat is 
        if ((int) currentProperties[seat.id.ToString()] != -1 && (int)currentProperties[seat.id.ToString()] != user)
            return false;

        currentProperties[seat.id.ToString()] = user;
        PhotonNetwork.CurrentRoom.SetCustomProperties(currentProperties);

        return true;
    }

    public static void UnoccupySeat(Seat seat)
    {
        if (!allSeats.Contains(seat))
            return;

        RefreshSeatStates();

        var currentProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        currentProperties[seat.id.ToString()] = -1;

        PhotonNetwork.CurrentRoom.SetCustomProperties(currentProperties);
    }

    public static void RefreshSeatStates()
    {
        var currentProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        List<int> users = new List<int>();
        List<string> invalidSeats = new List<string>();

        foreach (var player in FindObjectsOfType<PlayerMotionManager>())
            users.Add(player.photonView.ViewID);

        // Iterating through a dictionary is overly complex
        foreach (DictionaryEntry seat in currentProperties)
            if(!users.Contains((int) seat.Value) && (int)seat.Value != -1)
                invalidSeats.Add(seat.Key.ToString());

        foreach(var seat in invalidSeats)
            currentProperties[seat] = -1;

        PhotonNetwork.CurrentRoom.SetCustomProperties(currentProperties);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

        foreach (Seat seat in FindObjectsOfType<Seat>())
        {
            Handles.DrawBezier(
                transform.position, seat.transform.position, 
                transform.position + Vector3.down, seat.transform.position + Vector3.up, 
                (Selection.Contains(seat.gameObject) || Selection.Contains(gameObject)) ? Color.green : Color.white, 
                null, 2.0f);
        }
    }
#endif
}
