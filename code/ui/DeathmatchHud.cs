
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;
using ZombiePanic.ui;

[Library]
public partial class DeathmatchHud : HudEntity<RootPanel>
{
	public DeathmatchHud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/styles/hud.scss" );
 
		RootPanel.AddChild<Vitals>();
		RootPanel.AddChild<Ammo>();
		RootPanel.AddChild<Timerui>();
		RootPanel.AddChild<WhoWinui>();
		RootPanel.AddChild<ZomboVision>();

		RootPanel.AddChild<NameTags>();
		RootPanel.AddChild<DamageIndicator>();
		RootPanel.AddChild<HitIndicator>();

		RootPanel.AddChild<InventoryBar>();
		RootPanel.AddChild<PickupFeed>();
		RootPanel.AddChild<ActionMenuOpen>();

		RootPanel.AddChild<ChatBox>();
		RootPanel.AddChild<KillFeed>();
		RootPanel.AddChild<Scoreboard>();
		RootPanel.AddChild<VoiceList>();
	}

	[ClientRpc]
	public void OnPlayerDied( string victim, string attacker = null )
	{
		Host.AssertClient();
	}

	[ClientRpc]
	public void ShowDeathScreen( string attackerName )
	{
		Host.AssertClient();
	}
}
