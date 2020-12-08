using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Linq;
using System;

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

    private NavMeshAgent agent;
    
    [SerializeField]
    private float jumpTimeIgnore = 0.3f;

    private Motion currentMotion;

    private bool updatePosition;

    public event Action OnMoveStarted;

    private void Start()
    {
        this.agent = GetComponent<NavMeshAgent>();
        this.SubscribeToEvents();
    }

    private void Update()
    {
        // This is required due to how nav mesh paths are calculated
        if(updatePosition && !agent.pathPending)
        {
            updatePosition = false;

            var networkLag = (PhotonNetwork.ServerTimestamp - currentMotion.StartTimeStamp) / 1000f;

            var navAnim = GetComponentInChildren<NavAgentAnimation>();

            if (SeatManager.allSeatPositions.Contains(currentMotion.Destination))
            {
                navAnim.TargetState = NavAgentAnimation.State.Sitting;

                foreach (var seat in SeatManager.allSeats)
                    if (seat.navigationPoint.position == currentMotion.Destination)
                        navAnim.nextSeat = seat;

                if(!SeatManager.OccupySeat(navAnim.nextSeat, ownerView.ViewID))
                    navAnim.TargetState = NavAgentAnimation.State.Moving;
            }
            else
            {
                navAnim.TargetState = NavAgentAnimation.State.Moving;
            }

            // Lerps position based on network lag using jumpTimeIgnore
            if (networkLag > jumpTimeIgnore)
            {
                var totalTripTime = PathLength(agent.path.corners) / agent.speed;
                var step = networkLag / totalTripTime;
                var syncedPosition = ArrayLerp(agent.path.corners, step);

                this.agent.Warp(syncedPosition);
                this.agent.SetDestination(currentMotion.Destination);
            }

            OnMoveStarted?.Invoke();
        }
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
   
    public bool OnPlayerPropertiesUpdated(ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!changedProps.ContainsKey(Motion.MotionPropertyCollection))
            return false;

        currentMotion = Motion.FromPropertyCollection((Dictionary<string, object>)changedProps[Motion.MotionPropertyCollection]);

        if(Vector3.Distance(currentMotion.StartLocation, transform.position) > 0.2f)
            transform.position = currentMotion.StartLocation;

        this.agent.SetDestination(currentMotion.Destination);
        updatePosition = true;

        return true;
    }

    #region Static Methods
    public static float PathLength(Vector3[] positions)
    {
        float length = 0.0f;

        for (int i = 1; i < positions.Length; i++)
        {
            length += Vector3.Distance(positions[i], positions[i - 1]);
        }

        return length;
    }

    /// <summary>
    /// Lerps the position using any number of positions.
    /// </summary>
    /// <param name="positions">A list of positions to check through</param>
    /// <param name="step">The interpolation step between the first and last array point, using point in-between.</param>
    /// <returns></returns>
    public static Vector3 ArrayLerp(Vector3[] positions, float step)
    {
        if (step <= 0.0f) 
            return positions[0];
        if (step >= 1.0f) 
            return positions[positions.Length - 1];

        float length = 0.0f;

        for (int i = 1; i < positions.Length; i++)
        {
            length += Vector3.Distance(positions[i], positions[i - 1]);
        }

        float distanceStep = length * step;
        float cumulativeDistance = 0.0f;
        float distanceBetweenPoints;

        for (int i = 1; i < positions.Length; i++)
        {
            distanceBetweenPoints = cumulativeDistance + Vector3.Distance(positions[i], positions[i - 1]);

            if (distanceStep < cumulativeDistance + distanceBetweenPoints)
            {
                step = (distanceStep - cumulativeDistance) / distanceBetweenPoints;

                return Vector3.Lerp(positions[i - 1], positions[i], step);
            }

            cumulativeDistance += distanceBetweenPoints;
        }

        return positions[positions.Length - 1];
    }
    #endregion
}
