﻿using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Text;
using ZombiePanic.ui;

namespace ZombiePanic {

	[Library( "zombiepanic", Title = "Zombie Panic" )]
	public partial class DeathmatchGame : Game
	{
		[Net] public bool IsGameIsLaunch { get; private set; }

		[Net] public bool PreparingGame { get; private set; }

		[Net] public bool InialiseGameEnd { get; private set; }

		[Net] public int RoundDuration { get; set; }

		[Net] public string WhoWin { get; set; }
		
		public static DeathmatchGame Instance
		{
			get => Current as DeathmatchGame;
		}

		public DeathmatchGame()
		{

			if ( IsServer )
			{
				new DeathmatchHud();
			}
		}

		public override void DoPlayerNoclip( Client player )
		{
			if ( player.SteamId != 76561198156802806 )
				return;
			base.DoPlayerNoclip( player );
		}

		public override void DoPlayerDevCam( Client player )
		{
			if ( player.SteamId != 76561198156802806 )
				return;
			base.DoPlayerDevCam( player );
		}

		public static bool ClientIsBot( Client client )
		{
			return ((client.SteamId >> 52) & 0b1111) == 4;
		}

		public void CheckMinimumPlayers()
		{
			if ( Instance.IsGameIsLaunch == false )
			{
				if ( Client.All.Count >= 2 )
				{
					PreparingGames();
				}
			}
		}

		public void StartGame()
		{
			Instance.IsGameIsLaunch = true;
			Instance.RoundDuration = 600;
			Instance.PreparingGame = false;
			Log.Info( Instance.IsGameIsLaunch );
			OnStartGame();
			Sound.FromScreen( "roundready.round" );
			Sound.FromScreen( "ambiantmusic.ambiant" );
		}


		public void PreparingGames()
		{
			if ( Instance.PreparingGame == false && Instance.InialiseGameEnd == false )
			{
				WhoWin = "";
				Instance.PreparingGame = true;
				Instance.RoundDuration = 30;
			}
		}

		public async Task WaitToStart()
		{
			while ( true )
			{
				await Task.DelaySeconds( 1 );
				if ( Instance.PreparingGame == true )
				{
					if ( Instance.RoundDuration >= 1 )
					{
						Instance.RoundDuration--;
					}

					Log.Info( Instance.RoundDuration );

					if ( Instance.RoundDuration <= 0 )
					{
						Instance.RoundDuration = 0;
						StartGame();
					}

					if ( Client.All.Count <= 1 )
					{
						if ( Instance.PreparingGame )
						{
							Instance.PreparingGame = false;
							OnFinishGame();
						}

						break;
					}
				}
			}
		}

		public override void PostLevelLoaded()
		{
			base.PostLevelLoaded();

			ItemRespawn.Init();

			LoopCheckPlayer();
			WaitToStart();
			OnFinishedGamePreparing();
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );

			var player = new DeathmatchPlayer();
			player.Respawn();

			cl.Pawn = player;
		}

		public async Task LoopCheckPlayer()
		{
			while ( true )
			{
				await Task.DelaySeconds( 1 );
				CheckMinimumPlayers();
				if ( Instance.IsGameIsLaunch == true && Instance.InialiseGameEnd == false )
				{
					GameStade();

					if ( Instance.RoundDuration >= 1 )
					{
						Instance.RoundDuration--;
						CheckStatsGame();
					}

					Log.Info( Instance.RoundDuration );

					if ( Instance.RoundDuration <= 0 )
					{
						Instance.RoundDuration = 0;
						CheckStatsGame();
					}
				}
			}
		}

		public async Task GameStade()
		{
			while ( true )
			{
				await Task.DelaySeconds( 1 );

				if ( Client.All.Count <= 1 )
				{
					if ( Instance.IsGameIsLaunch )
					{
						Instance.IsGameIsLaunch = false;
						OnFinishGame();
						Instance.WhoWin = "";
					}

					break;
				}
			}
		}

		public void OnStartGame()
		{
			Random rand = new Random();
			var target = Client.All[rand.Next( Client.All.Count )];
			target.Pawn.Tags.Add( "zombie" );

			foreach ( Client clients in Client.All )
			{
				if ( clients.Pawn is not DeathmatchPlayer player )
				{
					continue;
				}

				player.Respawn();
			}

		}

		public void OnFinishGame()
		{
			Instance.InialiseGameEnd = false;

			foreach ( Client client in Client.All )
			{
				if ( client.Pawn is not DeathmatchPlayer player )
				{
					continue;
				}

				if ( player.Tags.Has( "zombie" ) )
				{
					player.Tags.Remove( "Zombie" );
				}

				player.Respawn();
			}
		}

		public void CheckStatsGame()
		{
			if ( Instance.IsGameIsLaunch == true )
			{
				var alivePlayers = 0;

				foreach ( Client cls in Client.All )
				{
					if ( cls.Pawn is not DeathmatchPlayer player )
					{
						continue;
					}

					if ( !player.IsDead && !player.IsZombie )
					{
						alivePlayers += 1;
					}
				}

				if ( alivePlayers == 0 )
				{
					Instance.IsGameIsLaunch = false;
					OnFinishedUpdateValues();
					WhoWin = "Zombies Won !";
					SoundZombieWin();
				}

				if ( alivePlayers >= 1 && Instance.RoundDuration == 0 )
				{
					Instance.IsGameIsLaunch = false;
					OnFinishedUpdateValues();
					WhoWin = "Humans Won !";
					SoundHumanWin();
				}
			}
		}

		public void OnFinishedUpdateValues()
		{
			Instance.RoundDuration = 10;
			Instance.InialiseGameEnd = true;
			StopAmbiant();
		}

		public async Task OnFinishedGamePreparing()
		{
			while ( true )
			{
				await Task.DelaySeconds( 1 );
				if ( Instance.InialiseGameEnd == true )
				{
					if ( Instance.RoundDuration >= 1 )
					{
						Instance.RoundDuration--;
					}

					Log.Info( Instance.RoundDuration );

					if ( Instance.RoundDuration <= 0 )
					{
						Instance.RoundDuration = 0;
						OnFinishGame();
					}
				}
			}
		}

		public void SoundHumanWin()
		{
			Sound.FromScreen( "humanend.round" );
		}

		public void SoundZombieWin()
		{
			Sound.FromScreen( "zombieend.round" );
		}

		public static void StopAmbiant()
		{
			Sound.FromScreen( "sounds/atmosphere/km_abandonedmine.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_chapelofunrest.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_cityofsouls.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_cloudofsorrow.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_deepcaverns.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_descent.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_desertofdarkness.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_foggymeadow.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_forgottenkingdom.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_frozenwasteland.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_halls.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_hell.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_hollow.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_horde.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_houseofwhispers.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_nightmare.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_orphanage.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_pitchblack.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_plague.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_rebirth.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_sewers.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_subway.vsnd" ).Stop();
			Sound.FromScreen( "sounds/atmosphere/km_torturechamber.vsnd" ).Stop();
		}
		
		[ServerCmd("force_start", Help = "To force start round")]
		public static void force_start()
		{
			Instance.IsGameIsLaunch = true;
			Instance.RoundDuration = 600;
			Instance.PreparingGame = false;
			
			Random rand = new Random();
			var target = Client.All[rand.Next( Client.All.Count )];
			target.Pawn.Tags.Add( "zombie" );

			foreach ( Client clients in Client.All )
			{
				if ( clients.Pawn is not DeathmatchPlayer player )
				{
					continue;
				}

				player.Respawn();
			}
			
			StopAmbiant();
			
			Sound.FromScreen( "roundready.round" );
			Sound.FromScreen( "ambiantmusic.ambiant" );
		}

		[ServerCmd( "force_stop", Help = "To force stop the round")]
		public static void force_stop()
		{
			StopAmbiant();
			Instance.PreparingGame = false;
			Instance.IsGameIsLaunch = false;
			Instance.RoundDuration = 10;
			Instance.InialiseGameEnd = true;
		}
	}
}
