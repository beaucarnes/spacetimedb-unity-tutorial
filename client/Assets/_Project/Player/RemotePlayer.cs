using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpacetimeDB.Types;
using TMPro;
using Unity.VisualScripting;

public class RemotePlayer : MonoBehaviour
{
    [DoNotSerialize]
    public ulong EntityId;

    public TMP_Text UsernameElement;

    public string Username { set { UsernameElement.text = value; } }

    void Start()
    {
        // Initialize overhead name
        UsernameElement = GetComponentInChildren<TMP_Text>();
        var canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;

        // Get the username from the PlayerComponent for this object and set it in the UI
        PlayerComponent? playerComp = PlayerComponent.FindByEntityId(EntityId);
        if (playerComp is null)
        {
            string inputUsername = UsernameElement.text;
            Debug.Log($"PlayerComponent not found - Creating a new player ({inputUsername})");
            Reducer.CreatePlayer(inputUsername);

            // Try again, optimistically assuming success for simplicity
            playerComp = PlayerComponent.FindByEntityId(EntityId);
        }

        Username = playerComp.Username;

        // Get the last location for this player and set the initial position
        EntityComponent entity = EntityComponent.FindByEntityId(EntityId);
        transform.position = new Vector3(entity.Position.X, entity.Position.Y, entity.Position.Z);

        // Register for a callback that is called when the client gets an
        // update for a row in the EntityComponent table
        EntityComponent.OnUpdate += EntityComponent_OnUpdate;
    }

    private void EntityComponent_OnUpdate(EntityComponent oldObj, EntityComponent obj, ReducerEvent callInfo)
    {
        // If the update was made to this object
        if(obj.EntityId == EntityId)
        {
            var movementController = GetComponent<PlayerMovementController>();

            // Update target position, rotation, etc.
            movementController.RemoteTargetPosition = new Vector3(obj.Position.X, obj.Position.Y, obj.Position.Z);
            movementController.RemoteTargetRotation = obj.Direction;
            movementController.SetMoving(obj.Moving);
        }
    }
}