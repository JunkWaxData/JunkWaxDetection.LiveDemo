using MatchType = JunkWaxDetection.LiveDemo.CardList.Enums.MatchType;

namespace JunkWaxDetection.LiveDemo.CardList
{
    public class CardSearchResult
    {
        /// <summary>
        ///     The type of Match that was found
        /// </summary>
        public MatchType Match { get; set; } = MatchType.None;

        /// <summary>
        ///     The Card that was found
        /// </summary>
        public Card Card { get; set; } = new();

        /// <summary>
        ///    The extracted text from the detected card
        /// </summary>
        public string ExtractedText { get; set; } = string.Empty;

        /// <summary>
        ///     The percentage of the extracted text that matches the card text
        /// </summary>
        public float Score { get; set; }
    }
}
