using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace ZombiePanic
{
	
	public enum Team : sbyte {Survivor = 0, Zombie = 1}
	
	public partial class Teams : Entity
	{
		private static readonly Color[] lightTeamColors_ =
		{
			new Color( 1f, 1f, 1f, 1f ), new Color( 0.5f, 0.7f, 1f, 1f ),
		};

		private static readonly Color[] darkTeamColors_ =
		{
			new Color( 0.5f, 0.5f, 0.5f, 1f ), new Color( 0.25f, 0.35f, 0.5f, 1f ),
		};
			
		private static readonly string[] longTeamNames_ =
		{
			"Survivor",
			"Zombie",
		};
		
		private static readonly string[] joinedStrings_ =
		{
			"has joined the Survivor",
			"has joined the Zombie",
		};
		
		[Net]
		private List<int> TeamPlayerCounts { get; set; }
		[Net]
		private List<Team> PlayerTeams { get; set; }
		
		public Teams()
		{
			TeamPlayerCounts = new List<int>( new int[4] );
			PlayerTeams = new List<Team>( new Team[64] );

			Transmit = TransmitType.Always;
		}
		
		public bool AssignClientToTeam( Client client, Team team )
		{
			// Do nothing if client already in the team
			if ( PlayerTeams[client.NetworkIdent - 1] == team )
			{
				return true;
			}

			// Update team player counts
			Team previousTeam = PlayerTeams[client.NetworkIdent - 1];
			TeamPlayerCounts[(int)previousTeam]--;

			PlayerTeams[client.NetworkIdent - 1] = team;
			TeamPlayerCounts[(int)team]++;

			if ( Host.IsServer )
			{
				Log.Info( $"\"{ client.Name }\" { joinedStrings_[(int)team] }" );
				ChatBox.AddInformation( To.Everyone, $"{ client.Name } { joinedStrings_[(int)team] }", $"avatar:{ client.SteamId }" );
			}

			return true;
		}
		
		public bool AutoAssignClient( Client client )
		{
			int zombiePlayerCount = TeamPlayerCounts[(int)Team.Zombie];
			int survivorPlayerCount = TeamPlayerCounts[(int)Team.Survivor];

			// Assign to the team with the least players
			if ( zombiePlayerCount > survivorPlayerCount )
			{
				return AssignClientToTeam( client, Team.Survivor );
			}
			else if ( zombiePlayerCount < survivorPlayerCount )
			{
				return AssignClientToTeam( client, Team.Zombie );
			}

			// If equal player counts, assign randomly
			return AssignClientToTeam( client, (Team)Rand.Int( 0, 1 ) );
		}

		public void OnClientConnected( Client client )
		{
			TeamPlayerCounts[(int)Team.Survivor]++;
		}

		public void OnClientDisconnected( Client client )
		{
			TeamPlayerCounts[(int)PlayerTeams[client.NetworkIdent - 1]]--;

			PlayerTeams[client.NetworkIdent - 1] = Team.Survivor;
		}

		public static int GetTeamPlayerCount( Team team )
		{
			Teams activeTeams = (Game.Current as DeathmatchGame)?.Teams;

			if ( activeTeams == null
			     || team < 0
			     || (int)team >= activeTeams.TeamPlayerCounts.Count )
			{
				return -1;
			}

			return activeTeams.TeamPlayerCounts[(int)team];
		}

		public Team GetClientTeam( Client client )
		{
			int startAtZeroNetworkId = client.NetworkIdent - 1;

			if ( startAtZeroNetworkId < 0 || startAtZeroNetworkId >= PlayerTeams.Count )
			{
				return Team.Survivor;
			}

			return PlayerTeams[startAtZeroNetworkId];
		}

		public static Color GetLightTeamColor( Team team )
		{
			if (team < 0 || (int)team >= lightTeamColors_.Length)
			{
				return lightTeamColors_[0];
			}

			return lightTeamColors_[(int)team];
		}

		public static Color GetDarkTeamColor( Team team )
		{
			if ( team < 0 || (int)team >= darkTeamColors_.Length )
			{
				return darkTeamColors_[0];
			}

			return darkTeamColors_[(int)team];
		}

		public static string GetLongTeamName( Team team )
		{
			if ( team < 0 || (int)team >= longTeamNames_.Length )
			{
				return "Unknown";
			}

			return longTeamNames_[(int)team]; 
		}
		
	}
}
