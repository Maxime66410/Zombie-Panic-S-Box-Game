using Sandbox;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public partial class Npc : AnimEntity
{
	public virtual float Speed => Rand.Float(100, 500);
	public virtual string ModelPath => "models/citizen/citizen.vmdl";
	public virtual float InitHealth => 0;
	public virtual bool HaveDress => true;
	public NavPath Path = new NavPath();

	DamageInfo lastDamage;

	ModelEntity pants;
	ModelEntity jacket;
	ModelEntity shoes;
	ModelEntity hat;

	public override void Spawn()
	{
		base.Spawn();

		Health = InitHealth;
		SetModel(ModelPath);
		EyePos = Position + Vector3.Up * 64;
		CollisionGroup = CollisionGroup.Player;
		SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius( 72, 8 ) );
		Tags.Add("npc");

		EnableHitboxes = true;

		if (HaveDress)
			Dress();
	}

	public override void OnKilled()
	{
		base.OnKilled();

		Game.Current?.OnKilled(this);

		if (lastDamage.Flags.HasFlag(DamageFlags.Vehicle))
		{
			Particles.Create("particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position);
			Particles.Create("particles/impact.flesh-big.vpcf", lastDamage.Position);
			PlaySound("kersplat");
		}

		BecomeRagdollOnClient(Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone(lastDamage.HitboxIndex));

		EnableAllCollisions = false;
		EnableDrawing = false;
	}

	public override void TakeDamage(DamageInfo info)
	{
		if (GetHitboxGroup(info.HitboxIndex) == 1)
		{
			info.Damage *= 2.0f;
		}

		lastDamage = info;

		base.TakeDamage(info);
 
		if (InitHealth <= 0) return;
		if ((info.Attacker != null && (info.Attacker is DeathmatchPlayer || info.Attacker.Owner is DeathmatchPlayer)))
		{
			DeathmatchPlayer attacker = info.Attacker as DeathmatchPlayer;

			if (attacker == null)
				attacker = info.Attacker.Owner as DeathmatchPlayer;

			// Note - sending this only to the attacker!
			//attacker.DidDamage(To.Single(attacker), info.Position, info.Damage, Health.LerpInverse(100, 0), Health <= 0);
		} 
	}

	[ClientRpc]
	private void BecomeRagdollOnClient(Vector3 velocity, DamageFlags damageFlags, Vector3 forcePos, Vector3 force, int bone)
	{
		var ent = new ModelEntity();
		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.Scale = Scale;
		ent.MoveType = MoveType.Physics;
		ent.UsePhysicsCollision = true;
		ent.EnableAllCollisions = true;
		ent.CollisionGroup = CollisionGroup.Debris;
		ent.SetModel(GetModelName());
		ent.CopyBonesFrom(this);
		ent.CopyBodyGroups(this);
		ent.CopyMaterialGroup(this);
		ent.TakeDecalsFrom(this);
		ent.EnableHitboxes = true;
		ent.EnableAllCollisions = true;
		ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ent.RenderColorAndAlpha = RenderColorAndAlpha;
		ent.PhysicsGroup.Velocity = velocity;

		ent.SetInteractsAs(CollisionLayer.Debris);
		ent.SetInteractsWith(CollisionLayer.WORLD_GEOMETRY);
		ent.SetInteractsExclude(CollisionLayer.Player | CollisionLayer.Debris);

		foreach (var child in Children)
		{
			if (child is ModelEntity e)
			{
				var model = e.GetModelName();
				if (model != null && !model.Contains("clothes"))
					continue;

				var clothing = new ModelEntity();
				clothing.SetModel(model);
				clothing.SetParent(ent, true);
				clothing.RenderColorAndAlpha = e.RenderColorAndAlpha;
			}
		}

		if (damageFlags.HasFlag(DamageFlags.Bullet) ||
			 damageFlags.HasFlag(DamageFlags.PhysicsImpact))
		{
			PhysicsBody body = bone > 0 ? ent.GetBonePhysicsBody(bone) : null;

			if (body != null)
			{
				body.ApplyImpulseAt(forcePos, force * body.Mass);
			}
			else
			{
				ent.PhysicsGroup.ApplyImpulse(force);
			}
		}

		if (damageFlags.HasFlag(DamageFlags.Blast))
		{
			if (ent.PhysicsGroup != null)
			{
				ent.PhysicsGroup.AddVelocity((Position - (forcePos + Vector3.Down * 100.0f)).Normal * (force.Length * 0.2f));
				var angularDir = (Rotation.FromYaw(90) * force.WithZ(0).Normal).Normal;
				ent.PhysicsGroup.AddAngularVelocity(angularDir * (force.Length * 0.02f));
			}
		}

		ent.DeleteAsync(10.0f);
	}

	public void Dress()
	{
		if (true)
		{
			var model = Rand.FromArray(new[]
			{
				"models/citizen_clothes/trousers/trousers.jeans.vmdl",
				"models/citizen_clothes/trousers/trousers.lab.vmdl",
				"models/citizen_clothes/trousers/trousers.police.vmdl",
				"models/citizen_clothes/trousers/trousers.smart.vmdl",
				"models/citizen_clothes/trousers/trousers.smarttan.vmdl",
				"models/citizen/clothes/trousers_tracksuit.vmdl",
				"models/citizen_clothes/trousers/trousers_tracksuitblue.vmdl",
				"models/citizen_clothes/trousers/trousers_tracksuit.vmdl",
				"models/citizen_clothes/shoes/shorts.cargo.vmdl",
			});

			pants = new ModelEntity();
			pants.SetModel(model);
			pants.SetParent(this, true);
			pants.EnableShadowInFirstPerson = true;
			pants.EnableHideInFirstPerson = true;

			SetBodyGroup("Legs", 1);
		}

		if (true)
		{
			var model = Rand.FromArray(new[]
			{
				"models/citizen_clothes/jacket/labcoat.vmdl",
				"models/citizen_clothes/jacket/jacket.red.vmdl",
				"models/citizen_clothes/jacket/jacket.tuxedo.vmdl",
				"models/citizen_clothes/jacket/jacket_heavy.vmdl",
			});

			jacket = new ModelEntity();
			jacket.SetModel(model);
			jacket.SetParent(this, true);
			jacket.EnableShadowInFirstPerson = true;
			jacket.EnableHideInFirstPerson = true;

			var propInfo = jacket.GetModel().GetPropData();
			if (propInfo.ParentBodyGroupName != null)
			{
				SetBodyGroup(propInfo.ParentBodyGroupName, propInfo.ParentBodyGroupValue);
			}
			else
			{
				SetBodyGroup("Chest", 0);
			}
		}

		if (true)
		{
			var model = Rand.FromArray(new[]
			{
				"models/citizen_clothes/shoes/trainers.vmdl",
				"models/citizen_clothes/shoes/shoes.workboots.vmdl"
			});

			shoes = new ModelEntity();
			shoes.SetModel(model);
			shoes.SetParent(this, true);
			shoes.EnableShadowInFirstPerson = true;
			shoes.EnableHideInFirstPerson = true;

			SetBodyGroup("Feet", 1);
		}

		if (true)
		{
			var model = Rand.FromArray(new[]
			{
				"models/citizen_clothes/hat/hat_hardhat.vmdl",
				"models/citizen_clothes/hat/hat_woolly.vmdl",
				"models/citizen_clothes/hat/hat_securityhelmet.vmdl",
				"models/citizen_clothes/hair/hair_malestyle02.vmdl",
				"models/citizen_clothes/hair/hair_femalebun.black.vmdl",
				"models/citizen_clothes/hat/hat_beret.red.vmdl",
				"models/citizen_clothes/hat/hat.tophat.vmdl",
				"models/citizen_clothes/hat/hat_beret.black.vmdl",
				"models/citizen_clothes/hat/hat_cap.vmdl",
				"models/citizen_clothes/hat/hat_leathercap.vmdl",
				"models/citizen_clothes/hat/hat_leathercapnobadge.vmdl",
				"models/citizen_clothes/hat/hat_securityhelmetnostrap.vmdl",
				"models/citizen_clothes/hat/hat_service.vmdl",
				"models/citizen_clothes/hat/hat_uniform.police.vmdl",
				"models/citizen_clothes/hat/hat_woollybobble.vmdl",
			});

			hat = new ModelEntity();
			hat.SetModel(model);
			hat.SetParent(this, true);
			hat.EnableShadowInFirstPerson = true;
			hat.EnableHideInFirstPerson = true;
		}
	}
}
