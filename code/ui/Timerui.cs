using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Timerui : Panel
{
	public Label Timer;

	Game game;
	
	public Timerui()
	{
		Timer = Add.Label( "Waiting Players" );
		Timer.SetClass("timer", true);
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

	}
}
