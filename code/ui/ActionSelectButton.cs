using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace ZombiePanic.ui
{
	public class ActionSelectButton : Button
	{
		protected string classAction_;
		public ActionSelectButton( string actionname )
		{
			classAction_ = actionname;

			string buttonLabelText = actionname;

			if ( actionname.Length != 0 )
			{
				buttonLabelText = actionname.Remove( 0, 1 ).Insert( 0, actionname[0].ToString().ToUpper() );
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
