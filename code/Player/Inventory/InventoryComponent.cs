using ClockBlockers.Anim;
using ClockBlockers.Timeline;
using Sandbox;
using System;
using System.Collections.Generic;
namespace ClockBlockers;
public partial class InventoryComponent : SimulatedComponent, ISingletonComponent
{
	[Net, Predicted] private Carriable? _networkedActiveChild { get; set; }

	public Carriable? ActiveChild
	{
		get => _networkedActiveChild; set
		{
			if ( value == _networkedActiveChild ) return;

			if ( _networkedActiveChild is Carriable prev ) prev.OnActiveEnd();

			if ( value is Carriable c ) c.OnActiveStart();
			_networkedActiveChild = value;
		}
	}
	[ClientInput] public Carriable? ActiveChildInput { get; set; }
	[Net] public IList<Carriable> Items { get; set; } = new List<Carriable>();
	public static int MaxItems { get; set; } = 32;

	[Predicted] Entity? PreviousActiveChild { get; set; }

	public bool AddActiveChild( Carriable item )
	{
		if ( !Items.Contains( item ) )
			if ( !AddItem( item ) ) return false;

		ActiveChild = item;
		return true;
	}

	public bool AddItem( Carriable item )
	{
		if ( item.Parent != null )
			throw new ArgumentException( "Selected item already has a parent.", nameof( item ) );
		if ( Items.Count < MaxItems )
		{
			Items.Add( item );
			if ( item is Carriable cr1 ) cr1.OnPickup( Entity );
			ActiveChildInput = item;
			return true;
		}
		return false;
	}

	public void PickupItem(Carriable item, bool recordAction = true)
	{
		if (ActiveChild != null && ActiveChild.CanDrop)
		{
			ThrowItem( ActiveChild );
		}

		if ( recordAction && Entity.IsRecording )
		{
			Entity.TimelineCapture?.Event( new PickupWeaponEvent( item ) );
			Entity.AnimCapture?.AddAction( new PickupWeaponAction( item ) );
		}
		AddActiveChild( item );
	}

	/// <summary>
	/// The velocity to apply to dropped weapons.
	/// </summary>
	protected static readonly Vector3 DROP_VELOCITY = new Vector3( 192, 0, 192 );

	public bool ThrowItem( Carriable? item, bool recordAction = false )
	{
		return DropItem( item, DROP_VELOCITY.RotateAround( Vector3.Zero, Entity.Transform.Rotation ), recordAction );
	}

	/// <summary>
	/// Remove an item from the inventory and drop it on the ground.
	/// </summary>
	/// <param name="item">The item to drop.</param>
	/// <param name="velocity">Velocity to add to the dropped item.</param>
	/// <param name="recordAction">Whether to record this as an action.</param>
	/// <returns>Success</returns>
	/// <exception cref="System.ArgumentException">If the weapon is not in our inventory.</exception>
	public bool DropItem( Carriable? item, Vector3 velocity = new Vector3(), bool recordAction = false )
	{
		if ( item == null ) return false;
		if ( !item.CanDrop ) return false;
		if ( item.Parent != Entity )
			throw new System.ArgumentException( "Item's parent is not this entity.", nameof( item ) );

		item.OnDrop( Entity );
		if ( ActiveChild == item )
		{
			ActiveChild = null;
		}
		if ( ActiveChildInput == item )
		{
			ActiveChildInput = null;
		}
		Items.Remove( item );

		//item.Position = Entity.AimRay.Position + Entity.AimRay.Forward * 48;
		//item.Velocity = Entity.AimRay.Forward * 200;

		item.Velocity += velocity;
		
		if (recordAction && Entity.IsRecording)
		{
			var action = new DropWeaponAction()
			{
				Velocity = velocity,
				WeaponID = item.GetPersistentIDOrCreate()
			};
			Entity.AnimCapture?.AddAction( action );
		}

		return true;
	}
	/// <summary>
	/// Get the item in this slot
	/// </summary>
	public virtual Carriable? GetSlot( int i )
	{
		if ( Items.Count <= i ) return null;
		if ( i < 0 ) return null;

		return Items[i];
	}
	/// <summary>
	/// Set our active entity to the entity on this slot
	/// </summary>
	public virtual bool SetActiveSlot( int i, bool evenIfEmpty = false )
	{
		var ent = GetSlot( i );
		if ( ActiveChild == ent )
			return false;

		if ( !evenIfEmpty && ent == null )
			return false;

		ActiveChild = ent;
		ActiveChildInput = ent;
		return ent.IsValid();
	}
	/// <summary>
	/// Returns the index of the currently active child
	/// </summary>
	public virtual int GetActiveSlot()
	{
		var ae = ActiveChild;
		var count = Items.Count;

		for ( int i = 0; i < count; i++ )
		{
			if ( Items[i] == ae )
				return i;
		}

		return -1;
	}
	/// <summary>
	/// Switch to the slot next to the slot we have active.
	/// </summary>
	public virtual bool SwitchActiveSlot( int idelta, bool loop )
	{
		var count = Items.Count;
		if ( count == 0 ) return false;

		var slot = GetActiveSlot();
		var nextSlot = slot + idelta;

		if ( loop )
		{
			while ( nextSlot < 0 ) nextSlot += count;
			while ( nextSlot >= count ) nextSlot -= count;
		}
		else
		{
			if ( nextSlot < 0 ) return false;
			if ( nextSlot >= count ) return false;
		}

		return SetActiveSlot( nextSlot, false );
	}

	public override void Simulate( IClient? cl )
	{
		base.Simulate( cl );

		// drop weapons

		if ( Entity.HasClient && Entity.ControlMethod == AgentControlMethod.Player && Input.Pressed( "Drop" ) && ActiveChild != null )
		{
			var item = ActiveChild;
			ThrowItem( item, true );
		}

		if ( ActiveChildInput.IsValid() && ActiveChildInput.Owner == Entity )
		{
			ActiveChild = ActiveChildInput;
		}
		else
		{
			ActiveChildInput = null;
			if ( Game.IsServer ) ActiveChild = null;
		}

		// Check to see if we've changed weapons
		//if ( ActiveChild != PreviousActiveChild )
		//{
		//	if ( PreviousActiveChild is Carriable cr1 ) cr1.OnActiveEnd();
		//	PreviousActiveChild = ActiveChild;
		//	if ( ActiveChild is Carriable cr2 ) cr2.OnActiveStart();
		//}

		if ( Entity.HasClient )
			ActiveChild?.Simulate( cl );
	}
	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
		ActiveChild?.FrameSimulate( cl );
	}
	public override void BuildInput()
	{
		base.BuildInput();
		ActiveChild?.BuildInput();
	}
	public virtual void SetActiveChild( int index )
	{

	}
	public virtual void SetActiveChild( Carriable entity )
	{
	}

	[ConCmd.Client]
	public static void ConCmdSetActiveChild( int i )
	{
		if ( ConsoleSystem.Caller.Pawn is PlayerAgent ply )
		{
		}
	}
}
