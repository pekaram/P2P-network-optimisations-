using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Motion
{
    public Vector3 Destination { get; set; }

    public Vector3 StartLocation { get; set; }

    public int StartTimeStamp { get; set; }

    public const string MotionPropertyCollection = "MotionCollectionName";

    public const string DestinationPropertyName = "Destination";

    public const string StartLocationPropertyName = "StartLocation";

    public const string StartTimestampPropertyName = "StartTimestamp";

    public Dictionary<string, object> GetPropertyCollection()
    {
        var motionProperties = new Dictionary<string, object>();

        motionProperties.Add(DestinationPropertyName, this.Destination);
        motionProperties.Add(StartLocationPropertyName, this.StartLocation);
        motionProperties.Add(StartTimestampPropertyName, this.StartTimeStamp);

        return motionProperties;
    }

    public static Motion FromPropertyCollection(Dictionary<string, object> motionProperties)
    {
        return new Motion()
        {
            Destination = (Vector3)motionProperties[DestinationPropertyName],
            StartLocation = (Vector3)motionProperties[StartLocationPropertyName],
            StartTimeStamp = (int)motionProperties[StartTimestampPropertyName]
        }; 
    }
}

public class PlayerMotionManager : MonoBehaviourPunCallbacks, IPlayerEventsListener
{
    [SerializeField]
    private PhotonView ownerView;
    
    private Motion activeMotion; 

    private float movingSpeedPerSecond = 1;

    private float playerLocationUpdateFrequency = 0.02f; 
    
    private void Start()
    {
        this.SubscribeToEvents();
        this.InvokeRepeating(nameof(this.UpdatePlayerLocation), 1, this.playerLocationUpdateFrequency);
    }

    private void SubscribeToEvents()
    {
        var dispatcher = FindObjectOfType<PlayerEventsDispatcher>();
        dispatcher.Subscribe(this.photonView.OwnerActorNr, this);

        if (ownerView.IsMine)
        {
            var inputHandler = FindObjectOfType<InputHandler>();
            inputHandler.OnLeftMouseClicked += OnLeftMouseClicked;
        }
    }
    

    private void OnLeftMouseClicked(Vector3 pressLocation)
    {
        this.SetNetworkMotion(pressLocation);
    }
    
    private void SetNetworkMotion(Vector3 location)
    {
        var motion = new Motion() { StartLocation = transform.position, Destination = location, StartTimeStamp = PhotonNetwork.ServerTimestamp };
        
        var setValue = new ExitGames.Client.Photon.Hashtable();
        setValue.Add(Motion.MotionPropertyCollection, motion.GetPropertyCollection());
        
        PhotonNetwork.LocalPlayer.SetCustomProperties(setValue);
    }
   
    private void UpdatePlayerLocation()
    {       
        if (this.activeMotion == null)
        {
            return;
        }

        this.transform.position = Vector3.MoveTowards(this.transform.position, this.activeMotion.Destination, playerLocationUpdateFrequency * movingSpeedPerSecond);
        
        var acceceptedDistanceToArrive = 0.001;
        if(Vector3.Distance(this.transform.position, this.activeMotion.Destination) < acceceptedDistanceToArrive)
        {
            Debug.Log("Motion ended");
            this.activeMotion = null;
        }
    }
    
    public bool OnPlayerPropertiesUpdated(ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!changedProps.ContainsKey(Motion.MotionPropertyCollection))
        {
            return false;
        }

        var motion = Motion.FromPropertyCollection((Dictionary<string, object>)changedProps[Motion.MotionPropertyCollection]);
        activeMotion = motion;

        // Lerp fast or so to start Location
        this.transform.position = motion.StartLocation;

        var timeInSecondsSinceMotionStarted = (PhotonNetwork.ServerTimestamp - motion.StartTimeStamp) / 1000f;
        var totalDistance = Vector3.Distance(motion.StartLocation, motion.Destination);
        var totalTripTime = totalDistance / movingSpeedPerSecond;

        var acceptedTreshold = 0.02;
        if (timeInSecondsSinceMotionStarted > acceptedTreshold)
        {
            var distanceCovered = (timeInSecondsSinceMotionStarted / totalTripTime) * totalDistance;
            var syncedPosition = Vector3.MoveTowards(motion.StartLocation, motion.Destination, distanceCovered);

            this.transform.position = syncedPosition;
        }

        return true;
    }
}
