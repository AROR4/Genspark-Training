using Wordle.Enums;

namespace Wordle.Services
{
    internal class WordProvider
    {
        protected List<string> _easy = [
            "APPLE", "BEACH", "BREAD", "CHAIR", "CLOUD", 
            "DREAM", "DRIVE", "EARTH", "FRUIT", "GLASS", 
            "GREEN", "HEART", "HOUSE", "LIGHT", "MUSIC", 
            "NIGHT", "OCEAN", "PLANT", "TABLE", "WATER"
        ];
        protected List<string> _medium=[
        "ADAPT", "ALERT", "BASIS", "BRAVE", "CABLE", 
        "CHASE", "DRAFT", "EAGER", "FLAME", "FORCE", 
        "GRANT", "IMAGE", "LEGAL", "MODEL", "PHONE", 
        "PRIDE", "QUIET", "RANGE", "SOUND", "TREND"];
        protected List<string> _hard = [
            "ABYSS", "ADIEU", "BLITZ", "CHASM", "CRYPT", 
            "FJORD", "GLYPH", "JUMPY", "LYNCH", "MYTHS", 
            "PIZZA", "QUART", "QUIRK", "SYNTH", "TANGO", 
            "TOXIC", "VIXEN", "WALTZ", "WHARF", "ZEBRA"
        ];
        
        private readonly Random _random = new Random();

        public string GetWord(Difficulty difficulty)
        {
            if(difficulty==Difficulty.Easy){
                int random = _random.Next(0,_easy.Count);
                return _easy[random];
            }
            else if (difficulty==Difficulty.Medium){
                int random = _random.Next(0,_medium.Count);
                return _medium[random];
            }
            else
            {
                int random = _random.Next(0,_hard.Count);
                return _hard[random];
            }
        }

        

    }
}