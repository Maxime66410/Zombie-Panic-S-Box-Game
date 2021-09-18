using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ZombiePanic.ui
{
	public class ZomboVision : Panel
	{
		public ZomboVision()
		{
			
		}

		public override void Tick()
		{
			var player = Local.Pawn as Player;
			if ( player == null ) return;

			if ( player.Tags.Has( "zombie" ) )
			{
				SetClass("zomboactive", true);
			}

			if ( player.Tags.Has( "human" ) )
			{
				SetClass("zomboactive", false);
			}
		}
	}
}
