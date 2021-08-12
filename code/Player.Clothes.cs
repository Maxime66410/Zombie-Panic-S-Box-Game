using Sandbox;
using System;
using System.Linq;

public class ClothingEntity : ModelEntity
{

}

partial class DeathmatchPlayer
{
	ModelEntity pants;
	ModelEntity jacket;
	ModelEntity shoes;
	ModelEntity hat;

	bool dressed = false;

	/// <summary>
	/// Bit of a hack to putr random clothes on the player
	/// </summary>
	public void Dress()
	{
		if ( dressed ) return;
		dressed = true;

		if ( Rand.Int( 0, 3 ) != 1 )
		{
			var model = Rand.FromArray( new[]
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
			} );

			pants = new ClothingEntity();
			pants.SetModel( model );
			pants.SetParent( this, true );
			pants.EnableShadowInFirstPerson = true;
			pants.EnableHideInFirstPerson = true;

			if ( model.Contains( "dress" ) )
				jacket = pants;
		}

		if ( Rand.Int( 0, 3 ) != 1 && jacket == null )
		{
			var model = Rand.FromArray( new[]
			{
				"models/citizen_clothes/jacket/labcoat.vmdl",
				"models/citizen_clothes/jacket/jacket.red.vmdl",
				"models/citizen_clothes/jacket/jacket.tuxedo.vmdl",
				"models/citizen_clothes/jacket/jacket_heavy.vmdl",
			} );

			jacket = new ClothingEntity();
			jacket.SetModel( model );
			jacket.SetParent( this, true );
			jacket.EnableShadowInFirstPerson = true;
			jacket.EnableHideInFirstPerson = true;
		}

		if ( Rand.Int( 0, 3 ) != 1 )
		{
			
			var model = Rand.FromArray( new[]
			{
				"models/citizen_clothes/shoes/trainers.vmdl",
				"models/citizen_clothes/shoes/shoes.workboots.vmdl"
			} );
			
			shoes = new ClothingEntity();
			shoes.SetModel( model );
			shoes.SetParent( this, true );
			shoes.EnableShadowInFirstPerson = true;
			shoes.EnableHideInFirstPerson = true;
		}

		if ( Rand.Int( 0, 3 ) != 1 )
		{
			var model = Rand.FromArray( new[]
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
			} );

			hat = new ClothingEntity();
			hat.SetModel( model );
			hat.SetParent( this, true );
			hat.EnableShadowInFirstPerson = true;
			hat.EnableHideInFirstPerson = true;
		}
	}
}
