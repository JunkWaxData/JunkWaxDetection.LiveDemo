namespace JunkWaxDetection.LiveDemo.CardList;

public interface ICardListController
{
    /// <summary>
    ///     Loads the latest card list for the specified set from the JunkWaxData Github Repository
    /// </summary>
    /// <param name="year"></param>
    /// <param name="setName"></param>
    bool LoadSet(string setLabel);

    (bool foundPlayer, Card card) HasPlayer(string setLabel, string playerString);

    /// <summary>
    ///     Cleans up the possible player name by removing common strings (such as position, acronyms, etc.)
    ///     from the string that OCR might read from the card.
    /// </summary>
    /// <param name="playerName"></param>
    /// <returns></returns>
    string CleanupString(string playerName);
}