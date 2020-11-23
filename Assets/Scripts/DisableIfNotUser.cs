using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableIfNotUser : MonoBehaviour
{
    public List<GameObject> objects;

    void Start()
    {
        bool user = GetComponent<Photon.Pun.PhotonView>().IsMine;

        foreach (GameObject o in objects)
            o.SetActive(user);
    }
}
