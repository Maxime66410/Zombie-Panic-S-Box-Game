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
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Any entities that implement this interface are added as a record and respawned
/// So it should really just be weapons, ammo and healthpacks etc
/// </summary>
public interface IRespawnableEntity
{

}

public class ItemRespawn
{
	/// <summary>
	/// A record of an entity and its position
	/// </summary>
	public class Record
	{
		public Transform Transform;
		public string ClassName;
	}

	/// <summary>
	/// a list of entity records
	/// </summary>
	static Dictionary<Entity, Record> Records = new();

	/// <summary>
	/// Create a bunch of records from the existing entities. This should be called before
	/// any players are spawned, but right after the level is loaded.
	/// </summary>
	public static void Init()
	{
		Records = new();

		foreach ( var entity in Entity.All )
		{
			if ( entity is IRespawnableEntity )
			{
				AddRecordFromEntity( entity );
			}
		}
	}

	/// <summary>
	/// Respawn this entity if it gets deleted (and Taken is called before!)
	/// </summary>
	/// <param name="ent"></param>
	public static void AddRecordFromEntity( Entity ent )
	{
		var record = new Record
		{
			Transform = ent.Transform,
			ClassName = ent.ClassInfo.Name
		};

		Records[ent] = record;
	}

	/// <summary>
	/// Entity has been picked up, or deleted or something.
	/// If it was in our records, start a respawn timer
	/// </summary>
	public static void Taken( Entity ent )
	{
		if ( Records.Remove( ent, out var record ) )
		{
			_ = RespawnAsync( record );
		}
	}

	/// <summary>
	/// Async Respawn timer. Wait 30 seconds, spawn the entity, add a record for it.
	/// </summary>
	static async Task RespawnAsync( Record record )
	{
		// TODO - Take.Delay In Game Time 
		await GameTask.Delay( 1000 * 30 );

		// TODO - find a sound that sounds like the echoey crazy truck horn sound that played in HL1 when items spawned
		Sound.FromWorld( "dm.item_respawn", record.Transform.Position + Vector3.Up * 50 );

		var ent = Library.Create<Entity>( record.ClassName );
		ent.Transform = record.Transform;

		Records[ent] = record;
	}
}
