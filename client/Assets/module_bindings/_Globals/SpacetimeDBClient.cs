// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.
// <auto-generated />

#nullable enable

using System;
using SpacetimeDB;
using SpacetimeDB.ClientApi;

namespace SpacetimeDB.Types
{
	public enum ReducerType
	{
		None,
		CreatePlayer,
		SendChatMessage,
		UpdatePlayerPosition,
	}

	public interface IReducerArgs : IReducerArgsBase
	{
		ReducerType ReducerType { get; }
		bool InvokeHandler(ReducerEvent reducerEvent);
	}

	public partial class ReducerEvent : ReducerEventBase
	{
		public IReducerArgs? Args { get; }

		public string ReducerName => Args?.ReducerName ?? "<none>";

		[Obsolete("ReducerType is deprecated, please match directly on type of .Args instead.")]
		public ReducerType Reducer => Args?.ReducerType ?? ReducerType.None;

		public ReducerEvent(IReducerArgs? args) : base() => Args = args;
		public ReducerEvent(TransactionUpdate update, IReducerArgs? args) : base(update) => Args = args;

		[Obsolete("Accessors that implicitly cast `Args` are deprecated, please match `Args` against the desired type explicitly instead.")]
		public CreatePlayerArgsStruct CreatePlayerArgs => (CreatePlayerArgsStruct)Args!;
		[Obsolete("Accessors that implicitly cast `Args` are deprecated, please match `Args` against the desired type explicitly instead.")]
		public SendChatMessageArgsStruct SendChatMessageArgs => (SendChatMessageArgsStruct)Args!;
		[Obsolete("Accessors that implicitly cast `Args` are deprecated, please match `Args` against the desired type explicitly instead.")]
		public UpdatePlayerPositionArgsStruct UpdatePlayerPositionArgs => (UpdatePlayerPositionArgsStruct)Args!;

		public override bool InvokeHandler() => Args?.InvokeHandler(this) ?? false;
	}

	public class SpacetimeDBClient : SpacetimeDBClientBase<ReducerEvent>
	{
		protected SpacetimeDBClient()
		{
			clientDB.AddTable<ChatMessage>();
			clientDB.AddTable<Config>();
			clientDB.AddTable<EntityComponent>();
			clientDB.AddTable<PlayerComponent>();
		}

		public static readonly SpacetimeDBClient instance = new();

		protected override ReducerEvent ReducerEventFromDbEvent(TransactionUpdate update)
		{
			var encodedArgs = update.ReducerCall.Args;
			IReducerArgs? args = update.ReducerCall.ReducerName switch {
				"CreatePlayer" => BSATNHelpers.Decode<CreatePlayerArgsStruct>(encodedArgs),
				"SendChatMessage" => BSATNHelpers.Decode<SendChatMessageArgsStruct>(encodedArgs),
				"UpdatePlayerPosition" => BSATNHelpers.Decode<UpdatePlayerPositionArgsStruct>(encodedArgs),
				"<none>" => null,
				"__identity_connected__" => null,
				"__identity_disconnected__" => null,
				"" => null,
				var reducer => throw new ArgumentOutOfRangeException("Reducer", $"Unknown reducer {reducer}")
			};
			return new ReducerEvent(update, args);
		}
	}
}
