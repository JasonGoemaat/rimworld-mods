# Thoughts

Try using [Traverse](https://harmony.pardeike.net/articles/utilities.html)
which should let me access private properties.

Try always returning true from `Pawn_PathFollower.AtDestinationPosition()`
or `Pawn.CanReachImmediate()` to make pawns SUPER fast.

		private bool AtDestinationPosition()
		{
			return this.pawn.CanReachImmediate(this.destination, this.peMode);
		}

The latter might be better as I can check if the pawn is a colonist.   This
is actually in an extension method on the pawn in the class `ReachabilityImmediate`:

		public static bool CanReachImmediate(this Pawn pawn, LocalTargetInfo target, PathEndMode peMode)

Looking at that, it may even be better for hauling.  It seems to use
LocalTargetInfo and may let you just pick things up from across the map?

Or maybe from Pawn_PathFollower I can just call private void `PatherArrived()`,
maybe from `private void Pawn_PathFollower.TryEnterNextPathCell()`?   That is called
at the end if AtDestinationPosition() returns true, so?







