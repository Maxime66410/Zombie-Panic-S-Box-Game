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


[Library( "dm_shotgun", Title = "Shotgun" )]
[Hammer.EditorModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" )]
partial class Shotgun : BaseDmWeapon
{ 
	public override string ViewModelPath => "weapons/rust_pumpshotgun/v_rust_pumpshotgun.vmdl";
	public override float PrimaryRate => 1;
	public override float SecondaryRate => 1;
	public override AmmoType AmmoType => AmmoType.ShotgunShells;
	public override int ClipSize => 8;
	public override float ReloadTime => 0.5f;
	public override int Bucket => 2;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" );  

		AmmoClip = 6;
	}

	public override void AttackPrimary() 
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( !TakeAmmo( 1 ) )
		{
			DryFire();
			return;
		}

		(Owner as AnimEntity).SetAnimBool( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "fireshotgun.shoot" );

		//
		// Shoot the bullets
		//
		Rand.SetSeed( Time.Tick );
		for ( int i = 0; i < 10; i++ )
		{
			ShootBullet( 0.15f, 0.3f, 9.0f, 3.0f );
		}
	}

	public override void AttackSecondary()
	{
		TimeSincePrimaryAttack = -0.5f;
		TimeSinceSecondaryAttack = -0.5f;

		if ( !TakeAmmo( 2 ) )
		{
			DryFire();
			return;
		}

		(Owner as AnimEntity).SetAnimBool( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		DoubleShootEffects();
		PlaySound( "fireshotgundouble.shoot" );

		//
		// Shoot the bullets
		//
		Rand.SetSeed( Time.Tick );
		for ( int i = 0; i < 20; i++ )
		{
			ShootBullet( 0.4f, 0.3f, 8.0f, 3.0f );
		}
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		ViewModelEntity?.SetAnimBool( "fire", true );

		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin(1.0f, 1.5f, 2.0f);
		}

		CrosshairPanel?.CreateEvent( "fire" );
	}

	[ClientRpc]
	protected virtual void DoubleShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimBool( "fire_double", true );
		CrosshairPanel?.CreateEvent( "fire" );

		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin(3.0f, 3.0f, 3.0f);
		}
	}

	public override void OnReloadFinish()
	{
		IsReloading = false;

		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( AmmoClip >= ClipSize )
			return;

		if ( Owner is DeathmatchPlayer player )
		{
			var ammo = player.TakeAmmo( AmmoType, 1 );
			if ( ammo == 0 )
				return;

			AmmoClip += ammo;

			PlaySound( "fireshotgunreloadsimple.reload" );
			
			if ( AmmoClip < ClipSize )
			{
				Reload();
			}
			else
			{
				FinishReload();
				PlaySound( "fireshotgunreloadfinish.reload" );
			}
		}
	}

	[ClientRpc]
	protected virtual void FinishReload()
	{
		ViewModelEntity?.SetAnimBool( "reload_finished", true );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 2 ); // TODO this is shit
		anim.SetParam( "aimat_weight", 1.0f );
	}
}
