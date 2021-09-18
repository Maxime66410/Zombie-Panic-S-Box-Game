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


[Library( "crossbow_bolt" )]
[Hammer.Skip]
partial class CrossbowBolt : ModelEntity
{
	bool Stuck;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_crossbow/rust_crossbow_bolt.vmdl" );
	}


	[Event.Tick.Server]
	public virtual void Tick()
	{
		if ( !IsServer )
			return;

		if ( Stuck )
			return;

		float Speed = 10000.0f;
		var velocity = Rotation.Forward * Speed;

		var start = Position;
		var end = start + velocity * Time.Delta;

		var tr = Trace.Ray( start, end )
				.UseHitboxes()
				//.HitLayer( CollisionLayer.Water, !InWater )
				.Ignore( Owner )
				.Ignore( this )
				.Size( 1.0f )
				.Run();


		if ( tr.Hit )
		{
			// TODO: CLINK NOISE (unless flesh)

			// TODO: SPARKY PARTICLES (unless flesh)

			Stuck = true;
			Position = tr.EndPos + Rotation.Forward * -1;

			if ( tr.Entity.IsValid() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPos, tr.Direction * 200, 60.0f )
													.UsingTraceResult( tr )
													.WithAttacker( Owner )
													.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}

			// TODO: Parent to bone so this will stick in the meaty heads
			SetParent( tr.Entity, tr.Bone );
			Owner = null;

			//
			// Surface impact effect
			//
			tr.Normal = Rotation.Forward * -1;
			tr.Surface.DoBulletImpact( tr );
			velocity = default;

			// delete self in 60 seconds
			_ = DeleteAsync( 60.0f );
		}
		else
		{
			Position = end;
		}
	}
}
