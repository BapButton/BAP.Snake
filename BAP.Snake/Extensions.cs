using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAP.Snake
{
    public static class Extensions
    {
        public static bool IsItemInSnake(this List<Location> list, Location newItem)
        {
            return list.Any(x => x.ColumnId == newItem.ColumnId && x.RowId == newItem.RowId);
        }
        public static bool IsItemInSnake(this List<Location> list, int rowId, int columnId)
        {
            return list.Any(x => x.ColumnId == columnId && x.RowId == rowId);
        }
        public static bool IsSnakeOverlapping(this List<Location> snake)
        {
            return snake.GroupBy(x => x).Any(g => g.Count() > 1);
        }

        public static bool IsSnakeEatingFood(this List<Location> snake, IEnumerable<Location> food)
        {
            return snake.Any(x => food.Contains(x));
        }
        public static void MoveSnake(this List<Location> snake, Direction direction, bool dontDeleteTail)
        {
            Location currentHeadLocation = new Location(snake.First().RowId, snake.First().ColumnId);
            currentHeadLocation.Move(direction);
            snake.Insert(0, currentHeadLocation);
            if (!dontDeleteTail)
            {
                snake.RemoveAt(snake.Count - 1);
            }
        }
        public static void MoveSnakeAndAddToTailOfSnake(this List<Location> snake, Direction direction)
        {
            snake.MoveSnake(direction, true);
        }
        public static bool IsSnakeHittingWall(this List<Location> snake, int maxColumnId, int maxRowId)
        {
            if (snake.Any(x => x.ColumnId > maxColumnId || x.RowId > maxRowId || x.ColumnId < 0 || x.RowId < 0))
            {
                return true;
            }
            return false;
        }

        public static bool IsOppositeDirection(this Direction direction, Direction? otherDirection)
        {
            if (otherDirection == null)
            {
                return false;
            }
            return (direction == Direction.Down && otherDirection == Direction.Up) ||
                (direction == Direction.Up && otherDirection == Direction.Down) ||
                (direction == Direction.Left && otherDirection == Direction.Right) ||
                (direction == Direction.Right && otherDirection == Direction.Left);
        }
    }
}
