using SpacetimeDB;
using SpacetimeDB.Types;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitcraftMiniGameManager : MonoBehaviour
{
    // These are connection variables that are exposed on the GameManager
    // inspector.
    [SerializeField] private string moduleAddress = "tutorial";
    [SerializeField] private string hostName = "localhost:3000";

    // This is the identity for this player that is automatically generated
    // the first time you log in. We set this variable when the
    // onIdentityReceived callback is triggered by the SDK after connecting
    private Identity local_identity;
    public static BitcraftMiniGameManager instance;

    public GameObject PlayerPrefab;
    public GameObject IronPrefab;

    [SerializeField] private GameObject preSpawnCamera;

    // Start is called before the first frame update
    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        Application.runInBackground = true;

        SpacetimeDBClient.instance.onConnect += () =>
        {
            Debug.Log("Connected.");

            // Request all tables
            SpacetimeDBClient.instance.Subscribe(new List<string>()
            {
                "SELECT * FROM *",
            });
        };

        // Called when we have an error connecting to SpacetimeDB
        SpacetimeDBClient.instance.onConnectError += (error, message) =>
        {
            Debug.LogError($"Connection error: {error} - {message}");
        };

        // Called when we are disconnected from SpacetimeDB
        SpacetimeDBClient.instance.onDisconnect += (closeStatus, error) =>
        {
            Debug.Log("Disconnected.");
        };

        // Called when we receive the client identity from SpacetimeDB
        SpacetimeDBClient.instance.onIdentityReceived += (token, identity, address) => {
            AuthToken.SaveToken(token);
            local_identity = identity;
        };

        // Called after our local cache is populated from a Subscribe call
        SpacetimeDBClient.instance.onSubscriptionApplied += OnSubscriptionApplied;

        // Now that we’ve registered all our callbacks, lets connect to spacetimedb
        SpacetimeDBClient.instance.Connect(AuthToken.Token, hostName, moduleAddress);

        PlayerComponent.OnInsert += PlayerComponent_OnInsert;
    }

    void OnSubscriptionApplied()
    {
        // If we don't have any data for our player, then we are creating a
        // new one. Let's show the username dialog, which will then call the
        // create player reducer
        var player = PlayerComponent.FindByIdentity(local_identity);
        if (player == null)
        {
          // Show username selection
          UIUsernameChooser.instance.Show();
        }

        // Show the Message of the Day in our Config table of the Client Cache
        UIChatController.instance.OnChatMessageReceived("Message of the Day: " + Config.FindByVersion(0).MessageOfTheDay);

        // Now that we've done this work we can unregister this callback
        SpacetimeDBClient.instance.onSubscriptionApplied -= OnSubscriptionApplied;

    }

    public void StartGame()
    {
        preSpawnCamera.SetActive(false);
        Reticle.instance.OnStart();
        UIChatController.instance.enabled = true;
    }

    private void PlayerComponent_OnInsert(PlayerComponent obj, ReducerEvent callInfo)
    {
        // If the identity of the PlayerComponent matches our user identity then this is the local player
        if(obj.Identity == local_identity)
        {
            // Now that we have our initial position we can start the game
            StartGame();
        }
        else
        {
            // Spawn the player object and attach the RemotePlayer component
            var remotePlayer = Instantiate(PlayerPrefab);

            // Lookup and apply the position for this new player
            var entity = EntityComponent.FindByEntityId(obj.EntityId);
            var position = new Vector3(entity.Position.X, entity.Position.Y, entity.Position.Z);
            remotePlayer.transform.position = position;

            var movementController = remotePlayer.GetComponent<PlayerMovementController>();
            movementController.RemoteTargetPosition = position;
            movementController.RemoteTargetRotation = entity.Direction;

            remotePlayer.AddComponent<RemotePlayer>().EntityId = obj.EntityId;
        }
    }
}
