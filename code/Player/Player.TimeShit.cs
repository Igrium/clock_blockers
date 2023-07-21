#nullable enable

using ClockBlockers.Anim;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers;

// All the timey-wimey shit involving remnants.

public partial class Player
{
	public AnimCapture? AnimCapture => Components.Get<AnimCapture>();
	public AnimPlayer? AnimPlayer => Components.Get<AnimPlayer>();

	/// <summary>
	/// Get or create the anim capture component.
	/// </summary>
	public AnimCapture CreateAnimCapture()
	{
		return Components.GetOrCreate<AnimCapture>();
	}

	/// <summary>
	/// Get or create the anim player component.
	/// </summary>
	public AnimPlayer CreateAnimPlayer()
	{
		return Components.GetOrCreate<AnimPlayer>();
	}

	/// <summary>
	/// We store IsGrounded explicitly so animations can set it when playing back.
	/// If this is a free agent, it is automatically set before each character animation tick.
	/// </summary>
	public bool IsGrounded { get; set; }

	public bool IsRecording => AnimCapture != null && AnimCapture.IsRecording;

	/// <summary>
	/// Tick all the time travel shit involving this agent.
	/// </summary>
	/// <param name="Client">The active client, if any.</param>
	public virtual void TickTimeShit( IClient? Client )
	{
		if ( IsFreeAgent ) IsGrounded = GroundEntity != null;

		var player = AnimPlayer;
		if ( player != null && ControlMethod == AgentControlMethod.PLAYBACK )
		{
			player.Tick();
		}

		var capture = AnimCapture;
		if ( capture != null ) capture.Tick();
	}

	public void StartCapture()
	{
		var animCapture = CreateAnimCapture();
		animCapture.Start();
	}

	public Animation StopCapture()
	{
		var anim = AnimCapture?.Stop();
		if ( anim == null )
			throw new InvalidOperationException( "Not recording." );
		return anim;
	}
}
