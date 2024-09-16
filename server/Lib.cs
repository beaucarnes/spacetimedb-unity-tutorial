using System.Runtime.CompilerServices;
using SpacetimeDB;
using static SpacetimeDB.Runtime;

static partial class Module
{
  [SpacetimeDB.Table(Public = true)]
  public partial struct Config {
      [SpacetimeDB.Column(ColumnAttrs.PrimaryKey)]
      public uint Version;
      public string? MessageOfTheDay;
  }

  /// This allows us to store 3D points in tables.
  [SpacetimeDB.Type]
  public partial struct StdbVector3
  {
    public float X;
    public float Y;
    public float Z;
  }

  /// This stores information related to all entities in our game. In this tutorial
  /// all entities must at least have an entity_id, a position, a direction and they
  /// must specify whether or not they are moving.
  [SpacetimeDB.Table(Public = true)]
  public partial struct EntityComponent
  {
    [SpacetimeDB.Column(ColumnAttrs.PrimaryKeyAuto)]
    public ulong EntityId;
    public StdbVector3 Position;
    public float Direction;
    public bool Moving;
  }

  /// All players have this component and it associates an entity with the user's
  /// Identity. It also stores their username and whether or not they're logged in.
  [SpacetimeDB.Table(Public = true)]
  public partial struct PlayerComponent
  {
    // An EntityId that matches an EntityId in the `EntityComponent` table.
    [SpacetimeDB.Column(ColumnAttrs.PrimaryKeyAuto)]
    public ulong EntityId;

    // The user's identity, which is unique to each player
    [SpacetimeDB.Column(ColumnAttrs.Unique)]
    public Identity Identity;
    public string? Username;
    public bool LoggedIn;
  }

  /// This reducer is called when the user logs in for the first time and
  /// enters a username.
  [SpacetimeDB.Reducer]
  public static void CreatePlayer(ReducerContext ctx, string username)
  {
    // Get the Identity of the client who called this reducer
    Identity sender = ctx.Sender;

    // Make sure we don't already have a player with this identity
    PlayerComponent? user = PlayerComponent.FindByIdentity(sender);
    if (user != null)
    {
        throw new ArgumentException("Player already exists");
    }

    // The PlayerComponent uses the same entity_id and stores the identity of
    // the owner, username, and whether or not they are logged in.
    try
    {
        new PlayerComponent
        {
            // EntityId = 0, // 0 is the same as leaving null to get a new, unique Id
            Identity = ctx.Sender,
            Username = username,
            LoggedIn = true,
        }.Insert();
    }
    catch
    {
        Log("Error: Failed to insert PlayerComponent", LogLevel.Error);
        throw;
    }

    var player = PlayerComponent.FindByIdentity(ctx.Sender)!.Value;
    
    // Create a new entity for this player
    try
    {
        new EntityComponent
        {
            EntityId = player.EntityId, // 0 is the same as leaving null to get a new, unique Id
            Position = new StdbVector3 { X = 0, Y = 0, Z = 0 },
            Direction = 0,
            Moving = false,
        }.Insert();
    }
    catch
    {
        Log("Error: Failed to create a unique EntityComponent", LogLevel.Error);
        throw;
    }

    Log($"Player created: {username}");
  }

  /// Called when the module is initially published
  [SpacetimeDB.Reducer(ReducerKind.Init)]
  public static void OnInit()
  {
    try
    {
        new Config
        {
            Version = 0,
            MessageOfTheDay = "Hello, World!",
        }.Insert();
    }
    catch
    {
        Log("Error: Failed to insert Config", LogLevel.Error);
        throw;
    }
  }

  /// Called when the client connects, we update the LoggedIn state to true
  [SpacetimeDB.Reducer(ReducerKind.Connect)]
  public static void ClientConnected(ReducerContext ctx) =>
    UpdatePlayerLoginState(ctx, loggedIn:true);

      
  /// Called when the client disconnects, we update the logged_in state to false
  [SpacetimeDB.Reducer(ReducerKind.Disconnect)]
  public static void ClientDisonnected(ReducerContext ctx) =>
    UpdatePlayerLoginState(ctx, loggedIn:false);

      
  /// This helper function gets the PlayerComponent, sets the LoggedIn
  /// variable and updates the PlayerComponent table row.
  private static void UpdatePlayerLoginState(ReducerContext ctx, bool loggedIn)
  {
    PlayerComponent? player = PlayerComponent.FindByIdentity(ctx.Sender);
    if (player is PlayerComponent p)
    {
        p.LoggedIn = loggedIn;
        PlayerComponent.UpdateByIdentity(ctx.Sender, p);
    }
    else
    {
        // If the user doesn't exist they must create a user by calling CreatePlayer().
        //throw new ArgumentException("Player not found");
    }
  }

  /// Updates the position of a player. This is also called when the player stops moving.
  [SpacetimeDB.Reducer]
  public static void UpdatePlayerPosition(
    ReducerContext ctx,
    StdbVector3 position,
    float direction,
    bool moving)
  {
    // First, look up the player using the sender identity
    PlayerComponent? player = PlayerComponent.FindByIdentity(ctx.Sender);
    if (player is PlayerComponent p)
    {
        // Use the Player's EntityId to retrieve and update the EntityComponent
        ulong playerEntityId = p.EntityId;
        EntityComponent? entity = EntityComponent.FindByEntityId(playerEntityId);
        if (entity is EntityComponent e)
        {
            e.Position = position;
            e.Direction = direction;
            e.Moving = moving;
            EntityComponent.UpdateByEntityId(playerEntityId, e);
        }
        else
        {
            throw new ArgumentException($"Player Entity '{playerEntityId}' not found");
        }
    }
    else
    {
        throw new ArgumentException("Player not found");
    }
  }

  [SpacetimeDB.Table(Public = true)]
  public partial struct ChatMessage
  {
    // The primary key for this table will be auto-incremented
    [SpacetimeDB.Column(ColumnAttrs.PrimaryKeyAuto)]

    // The entity id of the player that sent the message
    public ulong SenderId;

    // Message contents
    public string? Text;
  }

  /// Adds a chat entry to the ChatMessage table
  [SpacetimeDB.Reducer]
  public static void SendChatMessage(ReducerContext ctx, string text)
  {
    // Get the player's entity id
    PlayerComponent? player = PlayerComponent.FindByIdentity(ctx.Sender);
    if (player is PlayerComponent p)
    {
        // Insert the chat message
        new ChatMessage
        {
            SenderId = p.EntityId,
            Text = text,
        }.Insert();
    }
    else
    {
        throw new ArgumentException("Player not found");
    }
  }
}