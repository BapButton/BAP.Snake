using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAP.Snake
{
	public enum Direction
	{
		Up,
		Down,
		Left,
		Right
	}


	public class Location : IEquatable<Location>
	{
		public Location(int rowId, int columnId)
		{
			RowId = rowId;
			ColumnId = columnId;
		}

		public int RowId { get; set; }
		public int ColumnId { get; set; }

		public bool Equals(Location? other)
		{
			if (other is null)
			{
				return false;
			}

			// Optimization for a common success case.
			if (Object.ReferenceEquals(this, other))
			{
				return true;
			}

			// If run-time types are not exactly the same, return false.
			if (this.GetType() != other.GetType())
			{
				return false;
			}

			// Return true if the fields match.
			// Note that the base class is not invoked because it is
			// System.Object, which defines Equals as reference equality.
			return (RowId == other.RowId) && (ColumnId == other.ColumnId);
		}
		public override int GetHashCode() => (RowId, ColumnId).GetHashCode();
		public static bool operator ==(Location lhs, Location rhs)
		{
			if (lhs is null)
			{
				if (rhs is null)
				{
					return true;
				}

				// Only the left side is null.
				return false;
			}
			// Equals handles case of null on right side.
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Location lhs, Location rhs) => !(lhs == rhs);

		public void Move(Direction direction)
		{
			switch (direction)
			{
				case Direction.Up:
					RowId--;
					break;
				case Direction.Down:
					RowId++;
					break;
				case Direction.Left:
					ColumnId--;
					break;
				case Direction.Right:
					ColumnId++;
					break;
			}
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as Location);
		}
	}

}
