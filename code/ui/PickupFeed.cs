/*
	Zombie Panic! Source 2
	Created by Furrany Studio (Maxime66410)
	
	This project is Open Source, originale link : https://github.com/Maxime66410/Zombie-Panic-S-Box-Game
	
	Description :

	At the start of each round, players can either choose to join the human team or volunteer to be the first zombie. 
	If no one volunteers the game will pick one zombie randomly, and the game begins. The starter zombie's goal is to 
	kill the humans while the human goal is to stay alive as long as possible, complete objectives, or even wipe out 
	all the zombies. The catch is that when a human dies he will simply join the ranks of the undead, now ready to 
	finish off his old living teammates. The humans cannot tell by the player list who is alive and who is undead.
	
	Licenses :
		- Open Source : https://en.wikipedia.org/wiki/Open_source
		- CC BY : Attribution: depending on the NC, ND, and SA choices, others may share, edit, and use the model, but they must give you credit for the original work.
			-> https://creativecommons.org/licenses/by/4.0/
		- CC BY-NC : Non Commercial: others cannot use your model commercially
			-> https://creativecommons.org/licenses/by-nc/4.0/
		- CC BY-SA : ShareAlike: depending on the NC choices, others may share, edit, and use the model, but derivative work must be shared under the same license.
			-> https://creativecommons.org/licenses/by-sa/4.0/
*/


using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

public partial class PickupFeed : Panel
{
	public static PickupFeed Current;

	public PickupFeed()
	{
		Current = this;
	}

	/// <summary>
	/// An RPC which can be called from the server 
	/// </summary>
	[ClientRpc]
	public static void OnPickup( string text )
	{
		// TODO - icons for weapons?
		// TOPO - icons for ammo?

		Current?.AddEntry( $"\n{text}" );		
	}

	/// <summary>
	/// Spawns a label, waits for half a second and then deletes it
	/// The :outro style in the scss keeps it alive and fades it out
	/// </summary>
	private async Task AddEntry( string text )
	{
		var panel = Current.Add.Label( text );
		await Task.Delay( 500 );
		panel.Delete();
	}
}
