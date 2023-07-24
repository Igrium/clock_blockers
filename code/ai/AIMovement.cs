#nullable enable

using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockBlockers.AI;

public partial class LegacyAIController
{

	public static readonly float PATH_INTERVAL = .5f;

	public Vector3[]? Path { get; private set; }
	public TimeSince LastPathUpdate { get; private set; }
	public int CurrentPathSegment { get; private set; }


	public void TickMovement()
	{
		if ( Target == null ) return;

		LookAt( Target.Position );

		if (LastPathUpdate >= PATH_INTERVAL)
		{
			GeneratePath( Target.Position );
		}

		TraversePath();
	}

	public void MoveTowards( Vector3 target )
	{
		Entity.MovementDirection = (target - Entity.Position).Normal;
	}

	public void LookAt( Vector3 target )
	{
		var rot = Rotation.LookAt( (target - Entity.Position).Normal );
		Entity.ViewAngles = rot.Angles().WithRoll( 0 );
	}

	protected void GeneratePath( Vector3 target )
	{
		LastPathUpdate = 0;
		var entity = Entity;

		var builder = NavMesh.PathBuilder( entity.Position );
		builder = builder.WithMaxClimbDistance( 16 );
		builder = builder.WithMaxDropDistance( 64 );
		builder = builder.WithStepHeight( 16 );
		builder = builder.WithPartialPaths();

		var path = builder.Build( target );
		var segments = path.Segments;
		var segments1 = segments.Select( x => x.Position );

		Path = segments1.ToArray();

		//Path = NavMesh.PathBuilder( entity.Position )
		//	.WithMaxClimbDistance( 16f )
		//	.WithMaxDropDistance( 64f )
		//	.WithStepHeight( 16f )
		//	.WithMaxDistance( 999999 )
		//	.WithPartialPaths()
		//	.Build( target )
		//	.Segments
		//	.Select( x => x.Position )
		//	.ToArray();

		CurrentPathSegment = 0;
	}

	protected void TraversePath()
	{
		if ( Path == null ) return;

		var currentTarget = Path[CurrentPathSegment];
		MoveTowards( currentTarget );

		if (Entity.Position.Distance(currentTarget) <= 16f)
		{
			CurrentPathSegment++;
		}

		if (CurrentPathSegment >= Path.Count())
		{
			Path = null;
		}

	}
}
