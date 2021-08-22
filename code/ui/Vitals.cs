
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Vitals : Panel
{
	public Label Health;
	public Label Timer;
	public Label Team;

	public Vitals()
	{
		Health = Add.Label( "100", "health" );
		Team = Add.Label( "", "team" );
		Team.SetClass( "team", true );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

		Health.Text = $"{player.Health.CeilToInt()}";
		Health.SetClass( "danger", player.Health < 40.0f );

		if ( player.Tags.Has( "zombie" ) )
		{
			Team.Text = "Zombie";
			Team.SetClass( "teamzombie", true );
			return;
		}

		if ( player.Tags.Has( "human" ) )
		{
			Team.SetClass( "teamzombie", false );
			Team.SetClass( "team", true );
			Team.Text = "Human";
		}

	}
}
