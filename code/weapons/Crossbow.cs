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

[Library( "dm_crossbow", Title = "Crossbow" )]
[Hammer.EditorModel( "weapons/rust_crossbow/rust_crossbow.vmdl" )]
partial class Crossbow : BaseDmWeapon
{ 
	public override string ViewModelPath => "weapons/rust_crossbow/v_rust_crossbow.vmdl";

	public override float PrimaryRate => 1;
	public override int Bucket => 3;
	public override AmmoType AmmoType => AmmoType.Rifle;

	[Net]
	public bool Zoomed { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		AmmoClip = 3;
		SetModel( "weapons/rust_crossbow/rust_crossbow.vmdl" );
	}

	public override void AttackPrimary()
	{
		if ( !TakeAmmo( 1 ) )
		{
			DryFire();
			return;
		}

		ShootEffects();
		PlaySound( "firecrowbow.shoot" );

		if ( IsServer )
		using ( Prediction.Off() )
		{
			var bolt = new CrossbowBolt();
			bolt.Position = Owner.EyePos;
			bolt.Rotation = Owner.EyeRot;
			bolt.Owner = Owner;
			bolt.Velocity = Owner.EyeRot.Forward * 100;
		}
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Zoomed = Input.Down( InputButton.Attack2 );
	}

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		if ( Zoomed )
		{
			camSetup.FieldOfView = 20;
		}
	}

	public override void BuildInput( InputBuilder owner ) 
	{
		if ( Zoomed )
		{
			owner.ViewAngles = Angles.Lerp( owner.OriginalViewAngles, owner.ViewAngles, 0.2f );
		}
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		if ( Owner == Local.Pawn )
		{
			new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 1.0f, 0.5f );
		}

		ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}
}
