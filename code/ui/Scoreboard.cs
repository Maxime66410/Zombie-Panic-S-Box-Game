﻿/*
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
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Scoreboard : Sandbox.UI.Scoreboard<ScoreboardEntry>
{
	protected override void AddHeader()
	{
		Header = Add.Panel( "header" );
		Header.Add.Label( "player", "name" );
		Header.Add.Label( "kills", "kills" );
		Header.Add.Label( "deaths", "deaths" );
		Header.Add.Label( "ping", "ping" );
		Header.Add.Label( "fps", "fps" );
	}
}

public class ScoreboardEntry : Sandbox.UI.ScoreboardEntry
{
	public Label Fps;

	public ScoreboardEntry()
	{
		Fps = Add.Label( "", "fps" );
	}

	public override void UpdateFrom( PlayerScore.Entry entry )
	{
		base.UpdateFrom( entry );

		Fps.Text = entry.Get<int>( "fps", 0 ).ToString();
	}
}
