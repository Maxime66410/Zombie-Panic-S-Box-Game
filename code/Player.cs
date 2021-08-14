using Sandbox;
using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using ZombiePanic;

public partial class DeathmatchPlayer : Sandbox.Player
{
	TimeSince timeSinceDropped; 

	public bool SupressPickupNotices { get; private set; }
	[Net] public bool IsZombie { get; set; }
	[Net] public bool IsDead { get; set; }
	
	
	public DeathmatchPlayer()
	{
		Inventory = new DmInventory( this );
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
			IsZombie = false;
			SetMaterialGroup( 1 );
			RenderColor = Color.White;
			Inventory.DeleteContents();
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
		Random rnd = new Random();

		var RandomSound = rnd.Next( 0, 63 );
		
		if (RandomSound == 0)
		{
			PlaySound( "humanspawn01.spawn");
		}
		if (RandomSound == 1)
		{
			PlaySound( "humanspawn02.spawn");
		}
		if (RandomSound == 2)
		{
			PlaySound( "humanspawn03.spawn");
		}
		if (RandomSound == 3)
		{
			PlaySound( "humanspawn04.spawn");
		}
		if (RandomSound == 4)
		{
			PlaySound( "humanspawn05.spawn");
		}
		if (RandomSound == 5)
		{
			PlaySound( "humanspawn06.spawn");
		}
		if (RandomSound == 6)
		{
			PlaySound( "humanspawn07.spawn");
		}
		if (RandomSound == 7)
		{
			PlaySound( "humanspawn08.spawn");
		}
		if (RandomSound == 8)
		{
			PlaySound( "humanspawn09.spawn");
		}
		if (RandomSound == 9)
		{
			PlaySound( "humanspawn10.spawn");
		}
		if (RandomSound == 10)
		{
			PlaySound( "humanspawn11.spawn");
		}
		if (RandomSound == 11)
		{
			PlaySound( "humanspawn12.spawn");
		}
		if (RandomSound == 12)
		{
			PlaySound( "humanspawn13.spawn");
		}
		if (RandomSound == 13)
		{
			PlaySound( "humanspawn14.spawn");
		}
		if (RandomSound == 14)
		{
			PlaySound( "humanspawn15.spawn");
		}
		if (RandomSound == 15)
		{
			PlaySound( "humanspawn16.spawn");
		}
		if (RandomSound == 16)
		{
			PlaySound( "humanspawn17.spawn");
		}
		if (RandomSound == 17)
		{
			PlaySound( "humanspawn18.spawn");
		}
		if (RandomSound == 18)
		{
			PlaySound( "humanspawn19.spawn");
		}
		if (RandomSound == 19)
		{
			PlaySound( "humanspawn20.spawn");
		}
		if (RandomSound == 20)
		{
			PlaySound( "humanspawn21.spawn");
		}
		if (RandomSound == 21)
		{
			PlaySound( "humanspawn22.spawn");
		}
		if (RandomSound == 22)
		{
			PlaySound( "humanspawn23.spawn");
		}
		if (RandomSound == 23)
		{
			PlaySound( "humanspawn24.spawn");
		}
		if (RandomSound == 24)
		{
			PlaySound( "humanspawn25.spawn");
		}
		if (RandomSound == 25)
		{
			PlaySound( "humanspawn26.spawn");
		}
		if (RandomSound == 26)
		{
			PlaySound( "humanspawn27.spawn");
		}
		if (RandomSound == 27)
		{
			PlaySound( "humanspawn28.spawn");
		}
		if (RandomSound == 28)
		{
			PlaySound( "humanspawn29.spawn");
		}
		if (RandomSound == 29)
		{
			PlaySound( "humanspawn30.spawn");
		}
		if (RandomSound == 30)
		{
			PlaySound( "humanspawn31.spawn");
		}
		if (RandomSound == 31)
		{
			PlaySound( "humanspawn32.spawn");
		}
		if (RandomSound == 32)
		{
			PlaySound( "humanspawn33.spawn");
		}
		if (RandomSound == 33)
		{
			PlaySound( "humanspawn34.spawn");
		}
		if (RandomSound == 34)
		{
			PlaySound( "humanspawn35.spawn");
		}
		if (RandomSound == 35)
		{
			PlaySound( "humanspawn36.spawn");
		}
		if (RandomSound == 36)
		{
			PlaySound( "humanspawn37.spawn");
		}
		if (RandomSound == 37)
		{
			PlaySound( "humanspawn38.spawn");
		}
		if (RandomSound == 38)
		{
			PlaySound( "humanspawn39.spawn");
		}
		if (RandomSound == 39)
		{
			PlaySound( "humanspawn40.spawn");
		}
		if (RandomSound == 40)
		{
			PlaySound( "humanspawn41.spawn");
		}
		if (RandomSound == 41)
		{
			PlaySound( "humanspawn42.spawn");
		}
		if (RandomSound == 42)
		{
			PlaySound( "humanspawn43.spawn");
		}
		if (RandomSound == 43)
		{
			PlaySound( "humanspawn44.spawn");
		}
		if (RandomSound == 44)
		{
			PlaySound( "humanspawn45.spawn");
		}
		if (RandomSound == 45)
		{
			PlaySound( "humanspawn46.spawn");
		}
		if (RandomSound == 46)
		{
			PlaySound( "humanspawn47.spawn");
		}
		if (RandomSound == 47)
		{
			PlaySound( "humanspawn48.spawn");
		}
		if (RandomSound == 48)
		{
			PlaySound( "humanspawn49.spawn");
		}
		if (RandomSound == 49)
		{
			PlaySound( "humanspawn50.spawn");
		}
		if (RandomSound == 50)
		{
			PlaySound( "humanspawn51.spawn");
		}
		if (RandomSound == 51)
		{
			PlaySound( "humanspawn52.spawn");
		}
		if (RandomSound == 52)
		{
			PlaySound( "humanspawn53.spawn");
		}
		if (RandomSound == 53)
		{
			PlaySound( "humanspawn54.spawn");
		}
		if (RandomSound == 54)
		{
			PlaySound( "humanspawn55.spawn");
		}
		if (RandomSound == 55)
		{
			PlaySound( "humanspawn56.spawn");
		}
		if (RandomSound == 56)
		{
			PlaySound( "humanspawn57.spawn");
		}
		if (RandomSound == 57)
		{
			PlaySound( "humanspawn58.spawn");
		}
		if (RandomSound == 58)
		{
			PlaySound( "humanspawn59.spawn");
		}
		if (RandomSound == 59)
		{
			PlaySound( "humanspawn60.spawn");
		}
		if (RandomSound == 60)
		{
			PlaySound( "humanspawn61.spawn");
		}
		if (RandomSound == 61)
		{
			PlaySound( "humanspawn62.spawn");
		}
		if (RandomSound == 62)
		{
			PlaySound( "humanspawn63.spawn");
		}
		if (RandomSound == 63)
		{
			PlaySound( "humanspawn64.spawn");
		}

	}

	public void ZombieSpawnSound()
	{
		Random rnds = new Random();

		var RandomSound = rnds.Next( 0, 11 );
		
		if (RandomSound == 0)
		{
			PlaySound( "zombiespawn1.spawn");
		}
		if (RandomSound == 1)
		{
			PlaySound( "zombiespawn2.spawn");
		}
		if (RandomSound == 2)
		{
			PlaySound( "zombiespawn3.spawn");
		}
		if (RandomSound == 3)
		{
			PlaySound( "zombiespawn4.spawn");
		}
		if (RandomSound == 4)
		{
			PlaySound( "zombiespawn5.spawn");
		}
		if (RandomSound == 5)
		{
			PlaySound( "zombiespawn6.spawn");
		}
		if (RandomSound == 6)
		{
			PlaySound( "zombiespawn7.spawn");
		}
		if (RandomSound == 7)
		{
			PlaySound( "zombiespawn8.spawn");
		}
		if (RandomSound == 8)
		{
			PlaySound( "zombiespawn9.spawn");
		}
		if (RandomSound == 9)
		{
			PlaySound( "zombiespawn10.spawn");
		}
		if (RandomSound == 10)
		{
			PlaySound( "zombiespawn11.spawn");
		}
		if (RandomSound == 11)
		{
			PlaySound( "zombiespawn12.spawn");
		}
	}

	public void ZombieDieSound()
	{
		Random rnd = new Random();

		var RandomSound = rnd.Next( 0, 14 );
		
		if (RandomSound == 0)
		{
			PlaySound( "zombiedeath1.death");
		}
		if (RandomSound == 1)
		{
			PlaySound( "zombiedeath2.death");
		}
		if (RandomSound == 2)
		{
			PlaySound( "zombiedeath3.death");
		}
		if (RandomSound == 3)
		{
			PlaySound( "zombiedeath4.death");
		}
		if (RandomSound == 4)
		{
			PlaySound( "zombiedeath5.death");
		}
		if (RandomSound == 5)
		{
			PlaySound( "zombiedeath6.death");
		}
		if (RandomSound == 6)
		{
			PlaySound( "zombiedeath7.death");
		}
		if (RandomSound == 7)
		{
			PlaySound( "zombiedeath8.death");
		}
		if (RandomSound == 8)
		{
			PlaySound( "zombiedeath9.death");
		}
		if (RandomSound == 9)
		{
			PlaySound( "zombiedeath10.death");
		}
		if (RandomSound == 10)
		{
			PlaySound( "zombiedeath11.death");
		}
		if (RandomSound == 11)
		{
			PlaySound( "zombiedeath12.death");
		}
		if (RandomSound == 12)
		{
			PlaySound( "zombiedeath13.death");
		}
		if (RandomSound == 13)
		{
			PlaySound( "zombiedeath14.death");
		}
		if (RandomSound == 14)
		{
			PlaySound( "zombiedeath15.death");
		}
	}

	public void HumanDieSound()
	{
		Random rnd = new Random();

		var RandomSound = rnd.Next( 0, 39 );
		
		if (RandomSound == 0)
		{
			PlaySound( "humandeath01.die");
		}
		if (RandomSound == 1)
		{
			PlaySound( "humandeath02.die");
		}
		if (RandomSound == 2)
		{
			PlaySound( "humandeath03.die");
		}
		if (RandomSound == 3)
		{
			PlaySound( "humandeath04.die");
		}
		if (RandomSound == 4)
		{
			PlaySound( "humandeath05.die");
		}
		if (RandomSound == 5)
		{
			PlaySound( "humandeath06.die");
		}
		if (RandomSound == 6)
		{
			PlaySound( "humandeath06.die");
		}
		if (RandomSound == 7)
		{
			PlaySound( "humandeath08.die");
		}
		if (RandomSound == 8)
		{
			PlaySound( "humandeath09.die");
		}
		if (RandomSound == 9)
		{
			PlaySound( "humandeath10.die");
		}
		if (RandomSound == 10)
		{
			PlaySound( "humandeath11.die");
		}
		if (RandomSound == 11)
		{
			PlaySound( "humandeath12.die");
		}
		if (RandomSound == 12)
		{
			PlaySound( "humandeath13.die");
		}
		if (RandomSound == 13)
		{
			PlaySound( "humandeath14.die");
		}
		if (RandomSound == 14)
		{
			PlaySound( "humandeath15.die");
		}
		if (RandomSound == 15)
		{
			PlaySound( "humandeath16.die");
		}
		if (RandomSound == 16)
		{
			PlaySound( "humandeath17.die");
		}
		if (RandomSound == 17)
		{
			PlaySound( "humandeath18.die");
		}
		if (RandomSound == 18)
		{
			PlaySound( "humandeath19.die");
		}
		if (RandomSound == 19)
		{
			PlaySound( "humandeath20.die");
		}
		if (RandomSound == 20)
		{
			PlaySound( "humandeath21.die");
		}
		if (RandomSound == 21)
		{
			PlaySound( "humandeath22.die");
		}
		if (RandomSound == 22)
		{
			PlaySound( "humandeath23.die");
		}
		if (RandomSound == 23)
		{
			PlaySound( "humandeath24.die");
		}
		if (RandomSound == 24)
		{
			PlaySound( "humandeath25.die");
		}
		if (RandomSound == 25)
		{
			PlaySound( "humandeath26.die");
		}
		if (RandomSound == 26)
		{
			PlaySound( "humandeath27.die");
		}
		if (RandomSound == 27)
		{
			PlaySound( "humandeath28.die");
		}
		if (RandomSound == 28)
		{
			PlaySound( "humandeath29.die");
		}
		if (RandomSound == 29)
		{
			PlaySound( "humandeath30.die");
		}
		if (RandomSound == 30)
		{
			PlaySound( "humandeath31.die");
		}
		if (RandomSound == 31)
		{
			PlaySound( "humandeath32.die");
		}
		if (RandomSound == 32)
		{
			PlaySound( "humandeath33.die");
		}
		if (RandomSound == 33)
		{
			PlaySound( "humandeath34.die");
		}
		if (RandomSound == 34)
		{
			PlaySound( "humandeath35.die");
		}
		if (RandomSound == 35)
		{
			PlaySound( "humandeath36.die");
		}
		if (RandomSound == 36)
		{
			PlaySound( "humandeath37.die");
		}
		if (RandomSound == 37)
		{
			PlaySound( "humandeath38.die");
		}
		if (RandomSound == 38)
		{
			PlaySound( "humandeath39.die");
		}
		if (RandomSound == 39)
		{
			PlaySound( "humandeath40.die");
		}
	}
}
