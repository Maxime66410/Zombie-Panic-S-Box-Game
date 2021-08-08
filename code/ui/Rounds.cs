using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Rounds : Panel
{
	public Label Timer;
	public Label Round;

	public Rounds()
	{
		Timer = Add.Label( "Waiting Player" );
		Round = Add.Label( "" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;
		
		Timer.SetClass("timer", true);
		
	}
}
