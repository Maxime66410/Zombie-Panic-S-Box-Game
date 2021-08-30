using Sandbox;


[Library( "zp_winchester", Title = "Winchester" )]
[Hammer.EditorModel( "weapons/Shotsguns/winchester/winchester.vmdl" )]
partial class Winchester : BaseDmWeapon
{ 
	public override string ViewModelPath => "weapons/Shotsguns/winchester/v_winchester.vmdl";
	public override float PrimaryRate => 1;
	public override float SecondaryRate => 1;
	public override AmmoType AmmoType => AmmoType.ShotgunShells;
	public override int ClipSize => 2;
	public override float ReloadTime => 1.5f;
	public override int Bucket => 2;
	
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

		SetModel( "weapons/Shotsguns/winchester/winchester.vmdl" );  

		AmmoClip = 2;
		
		worldLight = CreateLight();
		worldLight.SetParent( this, "slide", new Transform( LightOffsetUp + LightOffsetForward ) );
		worldLight.EnableHideInFirstPerson = true;
		worldLight.Enabled = false;
	}
	
	public override void CreateViewModel()
	{
		base.CreateViewModel();

		viewLight = CreateLight();
		viewLight.SetParent( ViewModelEntity, "light", new Transform( ViewLightOffset ) );
		viewLight.EnableViewmodelRendering = true;
		viewLight.Enabled = LightEnabled;
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
		PlaySound( "firewinchester.shoot" );

		//
		// Shoot the bullets
		//
		Rand.SetSeed( Time.Tick );
		for ( int i = 0; i < 10; i++ )
		{
			ShootBullet( 0.15f, 2.3f, 9.0f, 3.0f );
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
		PlaySound( "firedoublewinchester.shoot" );

		//
		// Shoot the bullets
		//
		Rand.SetSeed( Time.Tick );
		for ( int i = 0; i < 20; i++ )
		{
			ShootBullet( 0.4f, 4.3f, 180.0f, 3.0f );
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

			PlaySound( "reloadsinglewinchester.reload" );
			
			if ( AmmoClip < ClipSize )
			{
				Reload();
			}
			else
			{
				FinishReload();
				PlaySound( "barrelclosewinchester.reload" );
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
