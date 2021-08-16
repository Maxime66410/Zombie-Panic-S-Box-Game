using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ZombiePanic.ui
{
	public class ActionSelectButton : Button
	{
		protected string classAction_;
		public ActionSelectButton( string actioname )
		{
			classAction_ = actioname;

			string buttonLabelText = actioname;

			if ( actioname.Length != 0 )
			{
				buttonLabelText = actioname.Remove( 0, 1 ).Insert( 0, actioname[0].ToString().ToUpper() );
			}

			Add.Label( buttonLabelText, "Label" );
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick(e);
			
			DeathmatchPlayer.HumanAction(classAction_);
		}
	}
}
