using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract partial class TimeRound : NetworkComponent
{
	public virtual string RoundName => "";
	
	public virtual int RoundDuration { get; set; } = 600;
	
	public float RoundEndTime { get; set; }

	public float TimeLeft
	{
		get
		{
			return RoundEndTime - Sandbox.Time.Now;
		}
	}
	
	public void Start()
	{
		if (Host.IsServer && RoundDuration > 0)
			RoundEndTime = Sandbox.Time.Now + RoundDuration;
		Log.Info( RoundEndTime );
	}
}
