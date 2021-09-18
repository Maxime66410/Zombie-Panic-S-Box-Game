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
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ZombiePanic.ui
{
	public class ActionMenuOpen : Panel
	{

		public static bool IsOpen;

		public string ChooseAction;
		public Button acknowledgebtn { get; private set; }
		public Button angerbtn { get; private set; }
		public Button campingbtn { get; private set; }
		public Button coverbtn { get; private set; }
		public Button declinebtn { get; private set; }
		public Button escapebtn { get; private set; }
		public Button firebtn { get; private set; }
		public Button gobtn { get; private set; }
		public Button holdbtn { get; private set; }
		public Button keepmovingbtn { get; private set; }
		public Button needammobtn { get; private set; }
		public Button needhealthbtn { get; private set; }
		public Button needweaponbtn { get; private set; }
		public Button positivestatusbtn { get; private set; }
		public Button praisebtn { get; private set; }
		public Button statusreportbtn { get; private set; }
		public Button tauntsbtn { get; private set; }
		public Button thanksbtn { get; private set; }

		public ActionMenuOpen()
		{
			CreatedButton();
		}

		public void CreatedButton()
		{
			acknowledgebtn = new Button( "acknowledge", "", () => ChooseAction = "acknowledge" );
			acknowledgebtn.AddClass( "buttonss" );
			angerbtn = new Button( "anger", "", () => ChooseAction = "anger" );
			angerbtn.AddClass( "buttonss" );
			campingbtn = new Button( "camping", "", () => ChooseAction = "camping" );
			campingbtn.AddClass( "buttonss" );
			coverbtn = new Button( "cover", "", () => ChooseAction = "cover" );
			coverbtn.AddClass( "buttonss" );
			declinebtn = new Button( "decline", "", () => ChooseAction = "decline" );
			declinebtn.AddClass( "buttonss" );
			escapebtn = new Button( "escape", "", () => ChooseAction = "escape" );
			escapebtn.AddClass( "buttonss" );
			firebtn = new Button( "fire", "", () => ChooseAction = "fire" );
			firebtn.AddClass( "buttonss" );
			gobtn = new Button( "go", "", () => ChooseAction = "go" );
			gobtn.AddClass( "buttonss" );
			holdbtn = new Button( "hold", "", () => ChooseAction = "hold" );
			holdbtn.AddClass( "buttonss" );
			keepmovingbtn = new Button( "keepmoving", "", () => ChooseAction = "keepmoving" );
			keepmovingbtn.AddClass( "buttonss" );
			needammobtn = new Button( "needammo", "", () => ChooseAction = "needammo" );
			needammobtn.AddClass( "buttonss" );
			needhealthbtn = new Button( "needhealth", "", () => ChooseAction = "needhealth" );
			needhealthbtn.AddClass( "buttonss" );
			needweaponbtn = new Button( "needweapon", "", () => ChooseAction = "needweapon" );
			needweaponbtn.AddClass( "buttonss" );
			positivestatusbtn = new Button( "positivestatus", "", () => ChooseAction = "positivestatus" );
			positivestatusbtn.AddClass( "buttonss" );
			praisebtn = new Button( "praise", "", () => ChooseAction = "praise" );
			praisebtn.AddClass( "buttonss" );
			statusreportbtn = new Button( "statusreport", "", () => ChooseAction = "statusreport" );
			statusreportbtn.AddClass( "buttonss" );
			tauntsbtn = new Button( "taunts", "", () => ChooseAction = "taunts" );
			tauntsbtn.AddClass( "buttonss" );
			thanksbtn = new Button( "thanks", "", () => ChooseAction = "thanks" );
			thanksbtn.AddClass( "buttonss" );


			AddChild( acknowledgebtn );
			AddChild( angerbtn );
			AddChild( campingbtn );
			AddChild( coverbtn );
			AddChild( declinebtn );
			AddChild( escapebtn );
			AddChild( firebtn );
			AddChild( gobtn );
			AddChild( holdbtn );
			AddChild( keepmovingbtn );
			AddChild( needammobtn );
			AddChild( needhealthbtn );
			AddChild( needweaponbtn );
			AddChild( positivestatusbtn );
			AddChild( praisebtn );
			AddChild( statusreportbtn );
			AddChild( tauntsbtn );
			AddChild( thanksbtn );
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "actives", IsOpen );

			var player = Local.Pawn as Player;
			if ( player == null ) return;
		}

		protected override void OnClick( MousePanelEvent e)
		{
			base.OnClick( e );
			
			DeathmatchPlayer.ActionName = ChooseAction;
			Log.Info(DeathmatchPlayer.ActionName);
		}

		public static void Checkclient( Client target )
		{
			if ( target == null )
			{
				return;
			}

			if ( Host.IsClient )
			{
				CheckMenu();
			}
		}

		public static void CheckMenu()
		{
			IsOpen = !IsOpen;
		}
		

		/*[Event.BuildInput]
		public void ProcessClientInput( InputBuilder input )
		{
			if ( !Players.IsZombie )
			{
				if ( input.Pressed( InputButton.Menu ) )
				{
					IsOpen = !IsOpen;
				}
			}
		}*/
	}
}
