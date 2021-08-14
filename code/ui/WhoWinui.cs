using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using ZombiePanic;


public class WhoWinui : Panel
{
	public Label WhoIsWin;
	
	public WhoWinui()
	{
		WhoIsWin = Add.Label( "" );
		WhoIsWin.SetClass("winwho", true);
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

		var timeofgame = DeathmatchGame.Instance;
		if ( timeofgame == null ) return;


		WhoIsWin.Text = timeofgame.WhoWin;
	}
}
