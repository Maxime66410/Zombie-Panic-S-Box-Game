using Sandbox;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Library("npc_zombie", Title = "Zombie", Spawnable = true )]
public partial class NpcZombie : Npc
{
	public override float Speed => 500;
	public override float InitHealth => 200;

	DeathmatchPlayer Target;
	TimeSince timeSinceAttPlayer;
 
	public override void Spawn()
	{
		base.Spawn();

		SetMaterialGroup(3);

		RenderColor = Color.Green;

		//SetAnimInt("holdtype", 2);
	}

	private void FindTarget()
    {
		var rply = All.OfType<DeathmatchPlayer>().ToArray();

		Target = rply[Rand.Int( 0, rply.Count() - 1 )];

		if ( Target.IsZombie )
		{
			FindTarget();
		}
    }
 
	private void AttPlayer()
    {
		var dmg = new DamageInfo()
		{
			Attacker = this,
			Position = Position,
			Damage = 5
		};

		Target.TakeDamage(dmg);
	}

	Vector3 InputVelocity;
	Vector3 LookDir;

	[Event.Tick.Server]
	public void Tick()
	{
		InputVelocity = 0;

		if (Target == null || Target.LifeState == LifeState.Dead)
			FindTarget();
		else
        {
			Path.Update(Position, Target.Position);

			if (!Path.IsEmpty)
            {
				var Direction = Path.GetDirection(Position);
				var avoid = GetAvoidance(Direction, Position, 500);
				if (!avoid.IsNearlyZero())
				{
					Direction = (Direction + avoid).Normal;
				}

				InputVelocity = Direction.Normal;
				Velocity = Velocity.AddClamped(InputVelocity * Time.Delta * 500, Speed);
			}

			if (Target.Position.Distance(Position) < 100 && timeSinceAttPlayer > 1f)
			{
				timeSinceAttPlayer = 0f;

				AttPlayer();
			}
		}

		Move( Time.Delta );

		var walkVelocity = Velocity.WithZ( 0 );
		if ( walkVelocity.Length > 0.5f )
		{
			var turnSpeed = walkVelocity.Length.LerpInverse( 0, 100, true );
			var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );
			Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 20.0f );
		}

		var animHelper = new CitizenAnimationHelper( this );

		LookDir = Vector3.Lerp( LookDir, InputVelocity.WithZ( 0 ) * 1000, Time.Delta * 100.0f );
		animHelper.WithLookAt( EyePos + LookDir );
		animHelper.WithVelocity( Velocity );
		animHelper.WithWishVelocity( InputVelocity );		
	}

	Vector3 GetAvoidance(Vector3 direction, Vector3 position, float radius)
	{
		var center = position + direction * radius * 0.5f;

		var objectRadius = 200.0f;
		Vector3 avoidance = default;

		foreach (var ent in Physics.GetEntitiesInSphere(center, radius))
		{
			if (ent.Parent == this || ent.Parent == Target) continue;
			if (ent.IsWorld) continue;

			var delta = (position - ent.Position).WithZ(0);
			var closeness = delta.Length;
			if (closeness < 0.001f) continue;
			var thrust = ((objectRadius - closeness) / objectRadius).Clamp(0, 1);
			if (thrust <= 0) continue;

			//avoidance += delta.Cross( Output.Direction ).Normal * thrust * 2.5f;
			avoidance += delta.Normal * thrust * thrust;
		}

		return avoidance;
	}

	protected virtual void Move( float timeDelta )
	{
		var bbox = BBox.FromHeightAndRadius( 64, 4 );

		MoveHelper move = new( Position, Velocity );
		move.MaxStandableAngle = 50;
		move.Trace = move.Trace.Ignore( this ).Size( bbox );

		if ( !Velocity.IsNearlyZero( 0.001f ) )
		{
			move.TryUnstuck();
			move.TryMoveWithStep( timeDelta, 30 );
		}

		var tr = move.TraceDirection( Vector3.Down * 10.0f );

		if ( move.IsFloor( tr ) )
		{
			GroundEntity = tr.Entity;

			if ( !tr.StartedSolid )
			{
				move.Position = tr.EndPos;
			}

			if ( InputVelocity.Length > 0 )
			{
				var movement = move.Velocity.Dot( InputVelocity.Normal );
				move.Velocity = move.Velocity - movement * InputVelocity.Normal;
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
				move.Velocity += movement * InputVelocity.Normal;

			}
			else
			{
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
			}
		}
		else
		{
			GroundEntity = null;
			move.Velocity += Vector3.Down * 900 * timeDelta;
		}

		Position = move.Position;
		Velocity = move.Velocity;
	}
}
