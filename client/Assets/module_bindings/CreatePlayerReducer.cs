// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.
// <auto-generated />

#nullable enable

using System;
using SpacetimeDB;

namespace SpacetimeDB.Types
{
	[SpacetimeDB.Type]
	public partial class CreatePlayerArgsStruct : IReducerArgs
	{
		ReducerType IReducerArgs.ReducerType => ReducerType.CreatePlayer;
		string IReducerArgsBase.ReducerName => "CreatePlayer";
		bool IReducerArgs.InvokeHandler(ReducerEvent reducerEvent) => Reducer.OnCreatePlayer(reducerEvent, this);

		public string Username = "";
	}

	public static partial class Reducer
	{
		public delegate void CreatePlayerHandler(ReducerEvent reducerEvent, string username);
		public static event CreatePlayerHandler? OnCreatePlayerEvent;

		public static void CreatePlayer(string username)
		{
			SpacetimeDBClient.instance.InternalCallReducer(new CreatePlayerArgsStruct { Username = username });
		}

		public static bool OnCreatePlayer(ReducerEvent reducerEvent, CreatePlayerArgsStruct args)
		{
			if (OnCreatePlayerEvent == null) return false;
			OnCreatePlayerEvent(
				reducerEvent,
				args.Username
			);
			return true;
		}
	}

}
