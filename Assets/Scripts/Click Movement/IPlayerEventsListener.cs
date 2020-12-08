using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerEventsListener 
{
    bool OnPlayerPropertiesUpdated(ExitGames.Client.Photon.Hashtable changedProps);
}
