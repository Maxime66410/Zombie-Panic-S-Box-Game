using Sandbox;
using Sandbox.UI;

namespace ZombiePanic.ui
{
	[Library]
	public class ActionCommand : HudEntity<RootPanel>
	{
		protected Panel buttonPanel_;
		public bool IsOpen = false;

		public ActionCommand()
		{
			if ( !IsClient )
			{
				return;
			}
			
			RootPanel.StyleSheet.Load("/styles/ActionMenuUI.scss");

			buttonPanel_ = RootPanel.AddChild<Panel>( "buttons" );

			Label label = RootPanel.AddChild<Label>( "select-action-label" );

			label.Text = "Select a Action";

			foreach ( string actionName in ClassAction.AllActions )
			{
				buttonPanel_.AddChild( new ActionSelectButton( actionName ) );
			}
			
			//Disable();
		}

		public void Enable()
		{
			RootPanel.Style.Display = DisplayMode.Flex;
			RootPanel.Style.PointerEvents = "all";
			RootPanel.Style.Dirty();
			IsOpen = true;

			foreach ( Panel child in buttonPanel_.Children )
			{
				child.Tick();
			}
		}

		public void Disable()
		{
			RootPanel.Style.Display = DisplayMode.None;
			RootPanel.Style.PointerEvents = "None";
			RootPanel.Style.Dirty();
			IsOpen = false;
		}
	}
}
