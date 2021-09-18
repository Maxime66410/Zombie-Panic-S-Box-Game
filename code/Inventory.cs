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
using System;
using System.Linq;

partial class DmInventory : BaseInventory
{


	public DmInventory( Player player ) : base ( player )
	{

	}

	public override bool Add( Entity ent, bool makeActive = false )
	{
		var player = Owner as DeathmatchPlayer;
		var weapon = ent as BaseDmWeapon;
		var notices = !player.SupressPickupNotices;
		//
		// We don't want to pick up the same weapon twice
		// But we'll take the ammo from it Winky Face
		//
		if ( weapon != null && IsCarryingType( ent.GetType() ) )
		{
			var ammo = weapon.AmmoClip;
			var ammoType = weapon.AmmoType;

			if ( ammo > 0 )
			{
				player.GiveAmmo( ammoType, ammo );

				if ( notices )
				{
					Sound.FromWorld( "dm.pickup_ammo", ent.Position );
					PickupFeed.OnPickup( To.Single( player ), $"+{ammo} {ammoType}" );
				}
			}

			ItemRespawn.Taken( ent );

			// Despawn it
			ent.Delete();
			return false;
		}

		if ( weapon != null && notices )
		{
			Sound.FromWorld( "dm.pickup_weapon", ent.Position );
			PickupFeed.OnPickup( To.Single( player ), $"{ent.ClassInfo.Title}" ); 
		}


		ItemRespawn.Taken( ent );
		return base.Add( ent, makeActive );
	}

	public bool IsCarryingType( Type t )
	{
		return List.Any( x => x.GetType() == t );
	}
}
