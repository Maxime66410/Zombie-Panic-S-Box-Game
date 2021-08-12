using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class TimerGameUI : Panel
{
	public Label Timer;
	public Label Round;

	public TimerGameUI()
	{
		Timer = Add.Label( "Waiting Player" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;
		
		Timer.SetClass("timer", true);
		
	}
}
