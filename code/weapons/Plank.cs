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

[Library("zp_plank", Title = "Plank", Spawnable = true)]
[Hammer.EditorModel("weapons/Melee/plank/plank.vmdl")]
partial class Plank : BaseDmWeapon
{
	public override string ViewModelPath => "weapons/Melee/plank/v_plank.vmdl";
	public override int ClipSize => -1;
	public override float PrimaryRate => 2.5f;
	public override float SecondaryRate => 0.5f;
	public override float ReloadTime => 4.0f;
	public override int Bucket => 0;
	public virtual int BaseDamage => 14;
	public virtual int MeleeDistance => 80;
	
	public override AmmoType AmmoType => AmmoType.Melee;
	
	protected virtual Vector3 LightOffsetUp => Vector3.Up * 10;
	protected virtual Vector3 LightOffsetForward => Vector3.Forward * 50;
	protected virtual Vector3 ViewLightOffset => Vector3.Forward * 10;

	private SpotLightEntity worldLight;
	private SpotLightEntity viewLight;

	[Net, Local, Predicted]
	private bool LightEnabled { get; set; } = true;

	TimeSince timeSinceLightToggled;

	public override void Spawn()
	{
		base.Spawn();

		SetModel("weapons/Melee/plank/plank.vmdl");
		
		worldLight = CreateLight();
		worldLight.SetParent( this, "slide", new Transform( LightOffsetUp + LightOffsetForward ) );
		worldLight.EnableHideInFirstPerson = true;
		worldLight.Enabled = false;
	}

	public virtual void MeleeStrike(float damage, float force)
	{
		var forward = Owner.EyeRot.Forward;
		forward = forward.Normal;

		foreach (var tr in TraceBullet(Owner.EyePos, Owner.EyePos + forward * MeleeDistance, 10f))
		{
			if (!tr.Entity.IsValid()) continue;

			tr.Surface.DoBulletImpact(tr);

			PlaySound( "plank-hit.hit" );

			if (!IsServer) continue;

			using (Prediction.Off())
			{
				var damageInfo = DamageInfo.FromBullet(tr.EndPos, forward * 100 * force, damage)
					.UsingTraceResult(tr)
					.WithAttacker(Owner)
					.WithWeapon(this);

				tr.Entity.TakeDamage(damageInfo);
			}
		}
	}

	public override void AttackPrimary()
	{
		//if (!CanPrimaryAttack()) return;
		
		PlaySound("throw.weee");
		MeleeStrike(BaseDamage, 1.5f);
		
		(Owner as AnimEntity).SetAnimBool("b_attack", true);
		ShootEffects();
	}
	

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		ViewModelEntity?.SetAnimBool("fire", true);
		CrosshairPanel?.CreateEvent("fire");
	}

	public override void SimulateAnimator(PawnAnimator anim)
	{
		anim.SetParam("holdtype", 4); // TODO this is shit
		anim.SetParam("holdtype_handedness", 1);
		anim.SetParam("aimat_weight", 1.0f);
	}
	
	public override void CreateViewModel()
	{
		base.CreateViewModel();

		viewLight = CreateLight();
		viewLight.SetParent( ViewModelEntity, "light", new Transform( ViewLightOffset ) );
		viewLight.EnableViewmodelRendering = true;
		viewLight.Enabled = LightEnabled;
	}
	
	private SpotLightEntity CreateLight()
	{
		var light = new SpotLightEntity
		{
			Enabled = true,
			DynamicShadows = true,
			Range = 512,
			Falloff = 1.0f,
			LinearAttenuation = 0.0f,
			QuadraticAttenuation = 1.0f,
			Brightness = 2,
			Color = Color.White,
			InnerConeAngle = 20,
			OuterConeAngle = 40,
			FogStength = 1.0f,
			Owner = Owner,
			LightCookie = Texture.Load( "materials/effects/lightcookie.vtex" )
		};

		return light;
	}
	
	public override void Simulate( Client cl )
	{
		if ( cl == null )
			return;

		base.Simulate( cl );

		bool toggle = Input.Pressed( InputButton.Flashlight );

		if ( timeSinceLightToggled > 0.1f && toggle )
		{
			LightEnabled = !LightEnabled;

			PlaySound( LightEnabled ? "flashlight-on" : "flashlight-off" );

			if ( worldLight.IsValid() )
			{
				worldLight.Enabled = LightEnabled;
			}

			if ( viewLight.IsValid() )
			{
				viewLight.Enabled = LightEnabled;
			}

			timeSinceLightToggled = 0;
		}
	}
	
	private void Activate()
	{
		if ( worldLight.IsValid() )
		{
			worldLight.Enabled = LightEnabled;
		}
	}
	
	private void Deactivate()
	{
		if ( worldLight.IsValid() )
		{
			worldLight.Enabled = false;
		}
	}
	
	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		if ( IsServer )
		{
			Activate();
		}
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );

		if ( IsServer )
		{
			if ( dropped )
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
		}
	}
}
