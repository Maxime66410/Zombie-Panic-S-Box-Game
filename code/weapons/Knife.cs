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

[Library("weapon_knife", Title = "Knife", Spawnable = true)]
[Hammer.EditorModel("weapons/rust_boneknife/rust_boneknife.vmdl")]
partial class Knife : BaseDmWeapon
{
	public override string ViewModelPath => "weapons/rust_boneknife/v_rust_boneknife.vmdl";
	public override int ClipSize => -1;
	public override float PrimaryRate => 1.0f;
	public override float SecondaryRate => 0.5f;
	public override float ReloadTime => 4.0f;
	public override int Bucket => 0;
	public virtual int BaseDamage => 35;
	public virtual int MeleeDistance => 80;

	public override void Spawn()
	{
		base.Spawn();

		SetModel("weapons/rust_boneknife/rust_boneknife.vmdl");
	}

	public virtual void MeleeStrike(float damage, float force)
	{
		var forward = Owner.EyeRot.Forward;
		forward = forward.Normal;

		foreach (var tr in TraceBullet(Owner.EyePos, Owner.EyePos + forward * MeleeDistance, 10f))
		{
			if (!tr.Entity.IsValid()) continue;

			tr.Surface.DoBulletImpact(tr);

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
		
		PlaySound("rust_boneknife.attack");
		MeleeStrike(BaseDamage, 1.5f);
		
		(Owner as AnimEntity).SetAnimBool("b_attack", true);
		ShootEffects();
	}

	public override void AttackSecondary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		(Owner as AnimEntity).SetAnimBool("b_attack", true);

		ShootEffects();
		PlaySound("rust_boneknife.attack");
		MeleeStrike(BaseDamage * 1.5f, 1.5f);
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
		anim.SetParam("aimat_weight", 1.0f);
	}
}
