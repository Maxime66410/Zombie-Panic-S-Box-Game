﻿using System;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PreparingGame : BaseRound
{
	public override string RoundName => "Preparing";

	public override int RoundDuration { get; set; } = 30;
	
	public override void OnPlayerLeave(DeathmatchPlayer player)
	{
		base.OnPlayerLeave(player);
	}
	
	public override void OnPlayerSpawn(DeathmatchPlayer player)
	{
		if (!Players.Contains(player))
			AddPlayer(player);
			
		base.OnPlayerSpawn(player);
	}
	
	protected override void OnStart()
	{
		foreach (var client in Client.All)
		{
			if (client.Pawn is not DeathmatchPlayer player)
				continue;

			player.Respawn();
		}
	}
	
	public override void OnTick()
	{
		base.OnTick();
	}

	public override void OnSecond()
	{
		base.OnSecond();
	}

	public override void OnTimeUp()
	{
		//DeathmatchGame.Instance.ChangeRound(new playingGame());
		base.OnTimeUp();
	}
}
