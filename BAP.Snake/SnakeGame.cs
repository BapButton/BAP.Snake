using BAP.Helpers;
using BAP.Types;
using MessagePipe;
using Microsoft.Extensions.Logging;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BAP.Snake
{
	internal class Snake : IBapGame, IDisposable
	{
		public bool IsGameRunning { get; set; }
		private ISubscriber<ButtonPressedMessage> ButtonPressedPipe { get; set; }
		IDisposable subscriptions = default!;
		ILogger<Snake> _logger;
		ILayoutProvider _layoutProvider;
		ButtonLayout _currentLayout;
		CancellationTokenSource timerTokenSource = new();
		int maxButtonRow = 0;
		int maxButtonColumn = 0;
		int maxRow = 0;
		int maxColumn = 0;
		List<Location> _snake = new();
		List<Location> _food = new();
		BapColor snakeColor = new(0, 255, 0);
		BapColor foodColor = new(200, 0, 0);
		IBapMessageSender _messageSender;
		private Direction? _nextDirection = null;
		private Direction _currentDirection = Direction.Down;
		PeriodicTimer? timer = null;
		int speedinMs = 500;


		public Snake(ILogger<Snake> logger, ISubscriber<ButtonPressedMessage> buttonPressed, IBapMessageSender messageSender, ILayoutProvider layoutProvider)
		{
			ButtonPressedPipe = buttonPressed;
			var bag = DisposableBag.CreateBuilder();
			ButtonPressedPipe.Subscribe(async (x) => await ButtonPressed(x)).AddTo(bag);
			subscriptions = bag.Build();
			_logger = logger;
			_layoutProvider = layoutProvider;
			_messageSender = messageSender;

		}

		public async Task ButtonPressed(ButtonPressedMessage buttonPressed)
		{
			if (IsGameRunning)
			{
				var buttonLocation = _layoutProvider?.CurrentButtonLayout?.ButtonPositions.FirstOrDefault(t => t.ButtonId == buttonPressed.NodeId);
				if (buttonLocation != null)
				{
					if (buttonLocation.ColumnId == 0)
					{
						_nextDirection = Direction.Left;
					}
					else if (buttonLocation.ColumnId == maxButtonColumn)
					{
						_nextDirection = Direction.Right;
					}
					else if (buttonLocation.RowId == 0)
					{
						_nextDirection = Direction.Up;
					}
					else if (buttonLocation.RowId == maxButtonRow)
					{
						_nextDirection = Direction.Down;
					}
				}
			}
		}

		public void UpdateTheButtonDisplay()
		{
			ulong[,] bigMatrix = new ulong[maxRow, maxColumn];
			List<(string nodeId, ButtonImage buttonImage)> images = new();

			for (int rowId = 0; rowId <= maxRow; rowId++)
			{
				for (int columnId = 0; columnId <= maxColumn; columnId++)
				{
					if (_snake.IsItemInSnake(rowId, columnId))
					{
						bigMatrix[rowId, columnId] = snakeColor.LongColor;
					}
					else if (_food.IsItemInSnake(rowId, columnId))
					{
						bigMatrix[rowId, columnId] = foodColor.LongColor;
					}
				}
			}
			//then loop through the buttons turning it into images
			for (int rowId = 0; rowId < maxButtonRow; rowId++)
			{
				for (int columnId = 0; columnId < maxButtonColumn; columnId++)
				{
					var currentbutton = _currentLayout.ButtonPositions.FirstOrDefault(t => t.RowId == rowId && t.ColumnId == columnId);
					if (currentbutton != null)
					{
						var image = new ButtonImage(bigMatrix.ExtractMatrix(rowId, (columnId * 8)));
						images.Add((currentbutton.ButtonId, image));
					}
				}
			}
			foreach (var image in images)
			{
				_messageSender.SendImage(image.nodeId, image.buttonImage);
			}
		}

		public void AddFood()
		{

			_food.Add(new Location(4, 4));
		}

		public void MoveSnake()
		{
			if (!_currentDirection.IsOppositeDirection(_nextDirection))
			{
				_currentDirection = _nextDirection ?? _currentDirection;
			}
			_snake.MoveSnake(_currentDirection);
			DecideIfWeHitAWall();
			DecideIfWeHitOurselves();
			DecideIfWeHitAFood();
		}

		public void DecideIfWeHitAWall()
		{
			if (_snake.IsSnakeHittingWall(maxRow, maxColumn))
			{
				EndGame("Snake hit a wall");
			}
		}

		public void DecideIfWeHitOurselves()
		{
			if (_snake.IsSnakeOverlapping())
			{
				EndGame("Snake ran into itself");
			}
		}



		public void DecideIfWeHitAFood()
		{
			//need to know what Food we hit so we can remove it from the food list. 
			if (_snake.IsSnakeEatingFood(_food))
			{
				_snake.MoveSnakeAndAddToTailOfSnake(_currentDirection);
				AddFood();
			}
		}

		public void Dispose()
		{
			subscriptions.Dispose();
		}

		private void EndGame(string v)
		{
			IsGameRunning = false;
			timer?.Dispose();
		}

		public Task<bool> ForceEndGame()
		{
			IsGameRunning = false;
			return Task.FromResult(true);
		}

		public Task<bool> Start()
		{
			IsGameRunning = true;
			maxButtonColumn = _layoutProvider?.CurrentButtonLayout?.ButtonPositions.Select(t => t.ColumnId).Max() ?? 0;
			maxButtonRow = _layoutProvider?.CurrentButtonLayout?.ButtonPositions.Select(t => t.RowId).Max() ?? 0;
			maxRow = maxButtonRow * 8;
			maxColumn = maxButtonColumn * 8;
			Task TimerTask = StartGameFrameTicker();
			_snake = new List<Location>();
			//need to find the mid Point
			_snake.Add(new Location(4, 4));
			_snake.MoveSnakeAndAddToTailOfSnake(Direction.Down);
			_snake.MoveSnakeAndAddToTailOfSnake(Direction.Down);
			_snake.MoveSnakeAndAddToTailOfSnake(Direction.Down);
			_currentDirection = Direction.Down;
			return Task.FromResult(true);
		}

		private async Task StartGameFrameTicker()
		{
			if (timerTokenSource.IsCancellationRequested)
			{
				timerTokenSource = new();

			}
			timer = new PeriodicTimer(TimeSpan.FromMilliseconds(speedinMs));
			var timerToken = timerTokenSource.Token;
			while (await timer.WaitForNextTickAsync(timerToken))
			{
				try
				{
					MoveSnake();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error in Move Snake Method");
				}
			};
			_logger.LogError("The Timer Has Stopped");
		}



	}
}
