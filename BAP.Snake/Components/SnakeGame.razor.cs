using BAP.Types;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAP.Snake.Components
{
    [GamePage("Snake Game", "Basic implementation of the classic Snake Game", "abe014f2-f5cf-4804-b135-c1856ead2aea")]
    public partial class SnakeGame
    {
        [Inject]
        IBapMessageSender MsgSender { get; set; } = default!;
        [Inject]
        IGameProvider GameHandler { get; set; } = default!;

        private string updateMessage = "";
        private string message = "";
        private Snake game { get; set; } = default!;

        protected override void OnInitialized()
        {

            base.OnInitialized();
            game = (Snake)GameHandler.UpdateToNewGameType(typeof(Snake));
        }

        void StartGame()
        {
            game.Start();
        }

        void EndGame()
        {
            game.ForceEndGame();
        }
    }
}
