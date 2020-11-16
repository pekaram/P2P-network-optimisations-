using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon;
using ExitGames.Client.Photon;

public class PlayerEventsDispatcher : MonoBehaviourPunCallbacks
{
    private Dictionary<int, IPlayerEventsListener> playerNumbersToCallBacks = new Dictionary<int, IPlayerEventsListener>();

    private Dictionary<int, ExitGames.Client.Photon.Hashtable> queuedCallbacks = new Dictionary<int, ExitGames.Client.Photon.Hashtable>();

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        this.playerNumbersToCallBacks[targetPlayer.ActorNumber].OnPlayerPropertiesUpdated(changedProps);

        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
    }

    public void Subscribe(int actorNumber, IPlayerEventsListener eventsListener)
    {
        this.playerNumbersToCallBacks.Add(actorNumber, eventsListener);
        if(queuedCallbacks.TryGetValue(actorNumber, out var changedProps))
        {
            this.playerNumbersToCallBacks[actorNumber].OnPlayerPropertiesUpdated(changedProps);
        }

        queuedCallbacks.Remove(actorNumber);
    }

    public void Unsubscribe(int actorNumber)
    {
        this.playerNumbersToCallBacks.Remove(actorNumber);
    }

    public override void OnJoinedRoom()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!playerNumbersToCallBacks.ContainsKey(player.ActorNumber))
            {
                queuedCallbacks.Add(player.ActorNumber, player.CustomProperties);
            }
            else
            {
                this.playerNumbersToCallBacks[player.ActorNumber].OnPlayerPropertiesUpdated(player.CustomProperties);
            }
        }
    }
}
