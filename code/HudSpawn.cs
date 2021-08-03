using Sandbox.UI;
using System;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace ZombiePanic
{
	/// <summary>
	/// This is the HUD entity. It creates a RootPanel clientside, which can be accessed
	/// via RootPanel on this entity, or Local.Hud.
	/// </summary>
	public partial class SpawnHudEntity : Sandbox.HudEntity<RootPanel>
	{

		public float lifeTime = 10.0f;
		public int a;
		public SpawnHudEntity()
		{
			if ( IsClient )
			{
				RootPanel.SetTemplate( "/SpawnHub.html" );
			}
		}

		public RemoveHud()
		{
			if ( IsClient )
			{
				 
			}
		}
	}

}
