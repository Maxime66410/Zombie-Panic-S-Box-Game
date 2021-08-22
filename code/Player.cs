using Sandbox;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using ZombiePanic;
using ZombiePanic.ui;
using System.Threading.Tasks;

public partial class DeathmatchPlayer : Sandbox.Player
{
	TimeSince timeSinceDropped; 

	public bool SupressPickupNotices { get; private set; }
	[Net] public bool IsZombie { get; set; }
	[Net] public bool IsDead { get; set; }
	
	[Net] public bool AlreadyGender { get; set; }
	
	[Net] public bool GenderType { get; set; }
	
	//[Net] public int ColorBody { get; set; } No need this for the moment
	[Net] public static string ActionName { get; set; } = "none";

	public DeathmatchPlayer()
	{
		Inventory = new DmInventory( this );
		HumanWaitAction();
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );
		
		if ( DeathmatchGame.Instance.IsGameIsLaunch  || DeathmatchGame.Instance.InialiseGameEnd)
		{
			if ( IsDead )
			{
				IsZombie = true;
				SetMaterialGroup( 1 );
				RenderColor = Color.Green;
			}

			if ( this.Tags.Has( "zombie" ) )
			{
				IsZombie = true;
				SetMaterialGroup( 1 );
				RenderColor = Color.Green;
				Inventory.DeleteContents();
			}

			if ( !AlreadyGender )
			{
				Random rnd = new Random();

				var RandomSound = rnd.Next( 0, 2 );

				Log.Info(RandomSound);
				
				if ( RandomSound == 0 )
				{
					GenderType = true;
					AlreadyGender = true;
				}

				if ( RandomSound == 1 )
				{
					GenderType = false;
					AlreadyGender = true;
				}
			}

			if ( !IsZombie )
			{
				HumanSpawnSound();
			}

			if ( IsZombie )
			{
				ZombieSpawnSound();
				this.Tags.Add( "zombie" );
			}

		}
		else
		{
			AlreadyGender = false;
			IsZombie = false;
			Inventory.DeleteContents();
			
			if ( !AlreadyGender )
			{
				Random rnd = new Random();

				var RandomGender = rnd.Next( 0, 2 );

				Log.Info(RandomGender);
				
				if ( RandomGender == 0 )
				{
					GenderType = true;
					AlreadyGender = true;
				}

				if ( RandomGender == 1 )
				{
					GenderType = false;
					AlreadyGender = true;
				}
			}

			if ( !IsZombie )
			{
			/*	SetMaterialGroup( 0 );
				
				Random rnds = new Random();

				var RandomColor = rnds.Next( 0, 2 );

				if ( RandomColor == 0 )
				{
					
				}*/
				SetMaterialGroup( 1 );
				RenderColor = Color.White;
			}
		}


		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();
		Camera = new FirstPersonCamera();
		  
		EnableAllCollisions = true; 
		EnableDrawing = true; 
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		

		Dress();
		ClearAmmo();

		SupressPickupNotices = true;

		if ( !IsZombie )
		{
			Inventory.Add( new Pistol(), true );
			Inventory.Add( new Shotgun() );
			Inventory.Add( new SMG() );
			Inventory.Add( new Crossbow() );
			Inventory.Add( new Knife() );
			
			GiveAmmo( AmmoType.Pistol, 100 );
			GiveAmmo( AmmoType.Buckshot, 8 );
			GiveAmmo( AmmoType.Crossbow, 4 );
		}

		if ( IsZombie )
		{
			Inventory.Add( new ZombieHand(), true );
			GiveAmmo( AmmoType.Pistol, 100 );
		}

		SupressPickupNotices = false;
		
		Health = 100;

		IsDead = false;
		
		base.Respawn();
	}
	public override void OnKilled()
	{
		base.OnKilled();

		if ( !IsZombie )
		{
			HumanDieSound();
		}
		else
		{
			ZombieDieSound();
		}
		
		if ( !IsZombie )
		{
			Inventory.DropActive();
		}
		Inventory.DeleteContents();

		BecomeRagdollOnClient( LastDamage.Force, GetHitboxBone( LastDamage.HitboxIndex ) );
		
		GetClientOwner().SetScore("deaths", GetClientOwner().GetScore<int>("deaths", 0) + 1);

		var LasAttacker = LastAttacker;

		if ( LasAttacker != null )
		{
			LasAttacker.GetClientOwner().SetScore("kills", LasAttacker.GetClientOwner().GetScore<int>("kills", 0) + 1);
		}

		Controller = null;
		Camera = new SpectateRagdollCamera();

		EnableAllCollisions = false;
		EnableDrawing = false;

		IsDead = true;
	}


	public override void Simulate( Client cl )
	{
		//if ( cl.NetworkIdent == 1 )
		//	return;

		base.Simulate( cl );

		//
		// Input requested a weapon switch
		//
		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
			return;

		if ( !IsZombie )
		{
			TickPlayerUse();
		}

		if ( Input.Pressed( InputButton.View ) )
		{
			if ( Camera is ThirdPersonCamera )
			{
				Camera = new FirstPersonCamera();
			}
			else
			{
				Camera = new ThirdPersonCamera();
			}
		}

		if ( !IsZombie )
		{
			if ( Input.Pressed( InputButton.Drop ) )
			{
				var dropped = Inventory.DropActive();
				if ( dropped != null )
				{
					if ( dropped.PhysicsGroup != null )
					{
						dropped.PhysicsGroup.Velocity = Velocity + (EyeRot.Forward + EyeRot.Up) * 300;
					}

					timeSinceDropped = 0;
					SwitchToBestWeapon();
				}
			}
			
			if ( Input.Pressed(InputButton.Menu) )
			{
				ActionMenuOpen.Checkclient(cl);
			}
			
			
			/*if ( ActionName != "none" )
			{
				HumanAction(ActionName);
			}*/
		}
		else
		{
			ActionMenuOpen.IsOpen = false;
		}

		SimulateActiveChild( cl, ActiveChild );

		//
		// If the current weapon is out of ammo and we last fired it over half a second ago
		// lets try to switch to a better wepaon
		//
		if ( ActiveChild is BaseDmWeapon weapon && !weapon.IsUsable() && weapon.TimeSincePrimaryAttack > 0.5f && weapon.TimeSinceSecondaryAttack > 0.5f )
		{
			SwitchToBestWeapon();
		}
	}

	public void SwitchToBestWeapon()
	{
		var best = Children.Select( x => x as BaseDmWeapon )
			.Where( x => x.IsValid() && x.IsUsable() )
			.OrderByDescending( x => x.BucketWeight )
			.FirstOrDefault();

		if ( best == null ) return;

		ActiveChild = best;
	}

	public override void StartTouch( Entity other )
	{
		if ( !IsZombie )
		{
			if ( timeSinceDropped < 1 ) return;
			base.StartTouch( other );
		}
	}

	Rotation lastCameraRot = Rotation.Identity;

	public override void PostCameraSetup( ref CameraSetup setup )
	{
		base.PostCameraSetup( ref setup );

		if ( lastCameraRot == Rotation.Identity )
			lastCameraRot = setup.Rotation;

		var angleDiff = Rotation.Difference( lastCameraRot, setup.Rotation );
		var angleDiffDegrees = angleDiff.Angle();
		var allowance = 20.0f;

		if ( angleDiffDegrees > allowance )
		{
			// We could have a function that clamps a rotation to within x degrees of another rotation?
			lastCameraRot = Rotation.Lerp( lastCameraRot, setup.Rotation, 1.0f - (allowance / angleDiffDegrees) );
		}
		else
		{
			//lastCameraRot = Rotation.Lerp( lastCameraRot, Camera.Rotation, Time.Delta * 0.2f * angleDiffDegrees );
		}

		// uncomment for lazy cam
		//camera.Rotation = lastCameraRot;

		if ( setup.Viewer != null )
		{
			AddCameraEffects( ref setup );
		}
	}

	float walkBob = 0;
	float lean = 0;
	float fov = 0;

	private void AddCameraEffects( ref CameraSetup setup )
	{
		var speed = Velocity.Length.LerpInverse( 0, 320 );
		var forwardspeed = Velocity.Normal.Dot( setup.Rotation.Forward );

		var left = setup.Rotation.Left;
		var up = setup.Rotation.Up;

		if ( GroundEntity != null )
		{
			walkBob += Time.Delta * 25.0f * speed;
		}

		setup.Position += up * MathF.Sin( walkBob ) * speed * 2;
		setup.Position += left * MathF.Sin( walkBob * 0.6f ) * speed * 1;

		// Camera lean
		lean = lean.LerpTo( Velocity.Dot( setup.Rotation.Right ) * 0.03f, Time.Delta * 15.0f );

		var appliedLean = lean;
		appliedLean += MathF.Sin( walkBob ) * speed * 0.2f;
		setup.Rotation *= Rotation.From( 0, 0, appliedLean );

		speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

		fov = fov.LerpTo( speed * 20 * MathF.Abs( forwardspeed ), Time.Delta * 2.0f );

		setup.FieldOfView += fov;

	//	var tx = new Sandbox.UI.PanelTransform();
	//	tx.AddRotation( 0, 0, lean * -0.1f );

	//	Hud.CurrentPanel.Style.Transform = tx;
	//	Hud.CurrentPanel.Style.Dirty(); 

	}

	DamageInfo LastDamage;

	public override void TakeDamage( DamageInfo info )
	{
		LastDamage = info;

		// hack - hitbox 0 is head
		// we should be able to get this from somewhere
		if ( info.HitboxIndex == 0 )
		{
			info.Damage *= 2.0f;
		}

		base.TakeDamage( info );

		if ( !IsZombie )
		{
			if ( GenderType )
			{
				PlaySound( "humansmalepain.pain" );
			}
			else
			{
				PlaySound( "humansfemalepain.pain" );
			}
		}
		else
		{
			PlaySound( "zombiepain.pain" );
		}

		if ( info.Attacker is DeathmatchPlayer attacker && attacker != this )
		{
			// Note - sending this only to the attacker!
			attacker.DidDamage( To.Single( attacker ), info.Position, info.Damage, Health.LerpInverse( 100, 0 ) );

			TookDamage( To.Single( this ), info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position );
		}
	}
	
	[ClientRpc]
	public void DidDamage( Vector3 pos, float amount, float healthinv )
	{
		Sound.FromScreen( "dm.ui_attacker" )
			.SetPitch( 1 + healthinv * 1 );

		HitIndicator.Current?.OnHit( pos, amount );
	}

	[ClientRpc]
	public void TookDamage( Vector3 pos )
	{
		//DebugOverlay.Sphere( pos, 5.0f, Color.Red, false, 50.0f );

		DamageIndicator.Current?.OnHit( pos );
	}

	public void HumanSpawnSound()
	{

		if ( GenderType )
		{
			PlaySound( "humansmalespawn.spawn");
		}
		else
		{
			PlaySound( "humanfemalespawn.spawn");
		}
	}

	public void ZombieSpawnSound()
	{
		PlaySound( "spawnzombie.spawn");
	}

	public void ZombieDieSound()
	{
		PlaySound( "zombiedeath.death");
	}

	public void HumanDieSound()
	{
		if ( GenderType )
		{
			PlaySound( "humanmaledeath.death");
		}
		else
		{
			PlaySound( "humanfemaledeath.death"); 
		}
	}

	public async Task HumanWaitAction()
	{
		while ( true )
		{
			await Task.DelaySeconds( 1 );

			if ( !IsZombie )
			{
				if ( ActionName != "none" )
				{
					HumanAction(ActionName);
				}
			}
		}
	}
	
	[ServerCmd]
	public void HumanAction(string nameOfAction)
	{
		if ( GenderType )
		{
			//PlaySound(nameOfAction + "males.action" );
			Sound.FromEntity( nameOfAction + "males.action", this);
			Log.Info(nameOfAction + "males.action");
			ActionName = "none";
		}
		else
		{
			//PlaySound(nameOfAction + "females.action" );
			Sound.FromEntity( nameOfAction + "females.action", this );
			Log.Info(nameOfAction + "females.action");
			ActionName = "none";
		}
	}
}
