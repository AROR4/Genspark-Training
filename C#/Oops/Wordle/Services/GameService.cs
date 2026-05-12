using Wordle.Enums;
using Wordle.Models;
using Wordle.Exceptions;
using Wordle.interfaces;
using Wordle.Repositories;

namespace Wordle.Services
{
        public class GameService : IGameService
        {
            private GameModel? _gameModel;
            ScoreRepository scoreRepository=new ScoreRepository();
            public void Start(
                int playerId,
                Difficulty difficulty)
            {
                WordProvider provider =
                    new WordProvider();

                string secretWord =
                    provider.GetWord(difficulty);

                _gameModel =
                    new GameModel(
                        playerId,
                        difficulty,
                        secretWord
                    );
            }

            public string SubmitGuess(
                string guess)
            {
                var game = _gameModel ??
                    throw new InvalidOperationException("Game has not been started.");

                GuessValidator.ValidateWord(
                    guess,
                    game.Guesses);

                string feedback =
                    FeedbackGenerator.GetAttemptFeedback(
                        game.SecretWord,
                        guess);

                game.Guesses.Add(guess);

                if(feedback == "GGGGG")
                {
                    game.IsWon = true;
                    game.feedback=FeedbackGenerator.GetFinalFeedback(game.AttemptsTaken+1);
                    game.Score =
                        FeedbackGenerator.CalculateScore(
                            game.AttemptsTaken + 1,
                            true);
                }

                game.AttemptsTaken++;
            if (game.AttemptsTaken == 6)
            {
                game.feedback=FeedbackGenerator.GetFinalFeedback(game.AttemptsTaken);
            }
                return feedback;
            }

            public void SaveGame()
            {
               
                var game=_gameModel ??
                    throw new InvalidOperationException("Game has not been started.");

                scoreRepository.SaveScore(_gameModel);
            }

            public GameModel GetGameState()
            {
                return _gameModel ??
                    throw new InvalidOperationException("Game has not been started.");
            }

    
    }

}

