namespace JunkWaxDetection.LiveDemo.CardList;

public interface ICardListController
{
    /// <summary>
    ///     Loads the latest card list for the specified set from the JunkWaxData Github Repository
    /// </summary>
    /// <param name="setLabel"></param>
    bool LoadSet(string setLabel);

    /// <summary>
    ///     Searches through the specified set for the player name extracted from the baseball card
    /// </summary>
    /// <param name="setLabel"></param>
    /// <param name="playerString"></param>
    /// <param name="cardSearchResult"></param>
    /// <returns></returns>
    bool CardSearch(string setLabel, string playerString, out CardSearchResult cardSearchResult);

    /// <summary>
    ///     Cleans up the possible player name by removing common strings (such as position, acronyms, etc.)
    ///     from the string that OCR might read from the card.
    /// </summary>
    /// <param name="playerName"></param>
    /// <returns></returns>
    string CleanupString(string playerName);
}