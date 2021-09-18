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

[Library( "dm_smg", Title = "SMG" )]
[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
partial class SMG : BaseDmWeapon
{ 
	public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";

	public override float PrimaryRate => 15.0f;
	public override float SecondaryRate => 1.0f;
	public override int ClipSize => 30;
	public override float ReloadTime => 4.0f;
	public override int Bucket => 2;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_smg/rust_smg.vmdl" );
		AmmoClip = 20;
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
		PlaySound( "firesmg.shoot" );

		//
		// Shoot the bullets
		//
		Rand.SetSeed(Time.Tick);
		ShootBullet( 0.1f, 1.5f, 7.0f, 3.0f );

	}

	public override void AttackSecondary()
	{
		// Grenade lob
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		if ( Owner == Local.Pawn )
		{
			new Sandbox.ScreenShake.Perlin(0.5f, 4.0f, 1.0f, 0.5f);
		}

		ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 2 ); // TODO this is shit
		anim.SetParam( "aimat_weight", 1.0f );
	}

}
