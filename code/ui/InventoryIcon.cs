﻿
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

class InventoryIcon : Panel
{
	public BaseDmWeapon Weapon;
	public Panel Icon;

	public InventoryIcon( BaseDmWeapon weapon )
	{
		Weapon = weapon;
		Icon = Add.Panel( "icon" );
		AddClass( weapon.ClassInfo.Name );
	}

	internal void TickSelection( BaseDmWeapon selectedWeapon )
	{
		SetClass( "active", selectedWeapon == Weapon );
		SetClass( "empty", !Weapon?.IsUsable() ?? true );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Weapon.IsValid() || Weapon.Owner != Local.Pawn )
			Delete();
	}
}
