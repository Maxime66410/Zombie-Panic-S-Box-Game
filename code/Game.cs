using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ZombiePanic {

  /// <summary>
  /// This is the heart of the gamemode. It's responsible
  /// for creating the player and stuff.
  /// </summary>
  [Library("dm98", Title = "DM98")]
  public partial class DeathmatchGame : Game
  {
	  [Net] public bool IsGameIsLaunch { get; private set; }

	  [Net]
    public Teams Teams {
      get;
      protected set;
    }

    public static DeathmatchGame Instance
    {
	    get => Current as DeathmatchGame;
    }
    public DeathmatchGame() {
      //
      // Create the HUD entity. This is always broadcast to all clients
      // and will create the UI panels clientside. It's accessible 
      // globally via Hud.Current, so we don't need to store it.
      //
      Teams = new Teams();

      if (IsServer) {
        new DeathmatchHud();
      }
    }
    
    public override void DoPlayerNoclip(Client player) {}

    public void CheckMinimumPlayers()
    {
	    if ( Client.All.Count >= 2 )
	    {
		    StartGame();
	    }
    }
    
    public void StartGame()
    {
	    Instance.IsGameIsLaunch = true;
    }
    
    
    public override void PostLevelLoaded() {
      base.PostLevelLoaded();

      ItemRespawn.Init();

      LoopCheckPlayer(); 
    }

    public override void ClientJoined(Client cl) {
      base.ClientJoined(cl);

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
		    GameStade();
	    }
    }

    public async Task GameStade()
    {
	    while ( true )
	    {
		    await Task.DelaySeconds( 1 );
		    if ( Client.All.Count <= 1 )
		    {
			    Instance.IsGameIsLaunch = false;
			    break;
		    }
	    }
    }

    public static void AutoJoinTeam(string teamName) {
     	Client target = ConsoleSystem.Caller;
	  
		if(target == null || Current is not DeathmatchGame deathmatchgame)
		{
			return;
		}

		if(teamName == "auto")
		{
			if(deathmatchgame.Teams.AutoAssignClient(target) && Host.IsServer)
			{
				return;
			}
		}

		Team team;
		switch (teamName)
		{
			case "Survivor":
				team = Team.Survivor;
				break;
			case "Zombie":
				team = Team.Zombie; 
				break;
			default:
				return;
		}
    }

	public static void TeamClassChange(Client target)
	{
		if(target == null || Current is not DeathmatchGame deathmatchgame)
		{
			return;
		}
	}

  }

}
