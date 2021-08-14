using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using ZombiePanic;


public class Timerui : Panel
{
	public Label Timer;
	
	public Timerui()
	{
		Timer = Add.Label( "Waiting Players" );
		Timer.SetClass("timer", true);
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

		var timeofgame = DeathmatchGame.Instance;
		if ( timeofgame == null ) return;

		if ( !timeofgame.PreparingGame && !timeofgame.InialiseGameEnd && !timeofgame.IsGameIsLaunch )
		{
			Timer.Text = "Waiting Players";
		}
		
		if (timeofgame.PreparingGame)
		{
			Timer.Text = "Game Start in : " + timeofgame.RoundDuration.ToString();
		}

		if ( timeofgame.IsGameIsLaunch )
		{
			Timer.Text = "Time  :  " + timeofgame.RoundDuration.ToString() + "  Seconds";
		}

		if ( timeofgame.InialiseGameEnd )
		{
			Timer.Text = "Round wil restart in  :  " + timeofgame.RoundDuration.ToString() + "  Seconds";
		}
	}
}
