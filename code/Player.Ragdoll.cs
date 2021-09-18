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

partial class DeathmatchPlayer
{
	// TODO - make ragdolls one per entity
	// TODO - make ragdolls dissapear after a load of seconds
	static EntityLimit RagdollLimit = new EntityLimit { MaxTotal = 20 };

	[ClientRpc]
	void BecomeRagdollOnClient( Vector3 force, int forceBone )
	{
		// TODO - lets not make everyone write this shit out all the time
		// maybe a CreateRagdoll<T>() on ModelEntity?
		var ent = new ModelEntity();
		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.MoveType = MoveType.Physics;
		ent.UsePhysicsCollision = true;
		ent.SetInteractsAs( CollisionLayer.Debris );
		ent.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		ent.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

		ent.SetModel( GetModelName() );
		ent.CopyBonesFrom( this );
		ent.TakeDecalsFrom( this );
		ent.SetRagdollVelocityFrom( this );
		ent.DeleteAsync( 20.0f );
		
		if ( IsZombie )
		{
			ent.SetMaterialGroup( 1 );
			ent.RenderColor = Color.Green;
		}
		else
		{
			// No need Config for the moment
		}

		// Copy the clothes over
		foreach ( var child in Children )
		{
			if ( child is ModelEntity e )
			{
				var model = e.GetModelName();
				if ( model != null && !model.Contains( "clothes" ) ) // Uck we 're better than this, entity tags, entity type or something?
					continue;

				var clothing = new ModelEntity();
				clothing.SetModel( model );
				clothing.SetParent( ent, true );
			}
		}

		ent.PhysicsGroup.AddVelocity( force );

		if ( forceBone >= 0 )
		{
			var body = ent.GetBonePhysicsBody( forceBone );
			if ( body != null )
			{
				body.ApplyForce( force * 1000 );
			}
			else
			{
				ent.PhysicsGroup.AddVelocity( force );
			}
		}

		Corpse = ent;

		RagdollLimit.Watch( ent );
	}
}
