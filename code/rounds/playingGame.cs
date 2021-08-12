using System;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class playingGame : BaseRound
{
	public override string RoundName => "Playing";

	public override int RoundDuration { get; set; } = 600;
	
	
	public override void OnPlayerLeave(DeathmatchPlayer player)
	{
		base.OnPlayerLeave(player);
	}
	
	public override void OnPlayerSpawn(DeathmatchPlayer player)
	{
		player.IsZombie = true;

		base.OnPlayerSpawn(player);
	}
	
	public override void OnPlayerKilled(DeathmatchPlayer player)
	{
		
		player.IsDead = true;

		player.GetClientOwner().SetScore("deaths", player.GetClientOwner().GetScore<int>("deaths", 0) + 1);

		var LastAttacker = player.LastAttacker;

		if (LastAttacker != null)
		{
			LastAttacker.GetClientOwner().SetScore("kills", LastAttacker.GetClientOwner().GetScore<int>("kills", 0) + 1);
		}

		var alivePlayers = 0;
		foreach (Client client in Client.All)
		{
			if (client.Pawn is not DeathmatchPlayer pl)
				continue;

			if (!pl.IsDead)
				alivePlayers += 1;
		}

		if (player.Tags.Has("murderer"))
		{
			//using (Prediction.Off())
				//Game.Instance.ShowWinner(To.Everyone, "Bystanders won !");

			//if (LastAttacker != null && LastAttacker.Tags.Has("detective"))
				LastAttacker.GetClientOwner().SetScore("kills", LastAttacker.GetClientOwner().GetScore<int>("kills", 0) + 1);

			/*Game.Instance.ChangeRound(new Ending() {
				Players = Players
			});*/
		}
		else
		{
			if (alivePlayers <= 1)
			{
				//using (Prediction.Off())
					//Game.Instance.ShowWinner(To.Everyone, "Murderer won !");

				/*Game.Instance.ChangeRound(new Ending() {
					Players = Players
				});*/
			}
		}
	}

	protected override void OnStart()
	{
		Random rand = new Random();
		var target = Client.All[rand.Next(Client.All.Count)];
		base.OnStart();
	}
}
