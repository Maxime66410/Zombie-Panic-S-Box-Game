using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Timerui : Panel
{
	public Label Timer;
	public Timerui()
	{
		Timer = Add.Label( "Waiting Players" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;
		
		Timer.SetClass("timer", true);
		
	}
}
