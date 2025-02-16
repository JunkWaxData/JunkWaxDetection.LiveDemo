using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using MatchType = JunkWaxDetection.LiveDemo.CardList.Enums.MatchType;

namespace JunkWaxDetection.LiveDemo.CardList
{
    /// <summary>
    ///     This class utilizes the JSON Data Sets from Junk Wax Data (https://github.com/JunkWaxData) to
    ///     search through specified card sets for the player names extracted from the baseball card being
    ///     evaluated.
    ///
    ///     Because the text is extracted using OCR, we need to clean up the text to remove any artifacts or
    ///     common strings 
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="httpClient"></param>
    public class CardListController(IOptions<AppSettings> appSettings, HttpClient httpClient) : ICardListController
    {
        private readonly Dictionary<string, CardList> _cardLists = new();

        /// <summary>
        ///     Loads the latest card list for the specified set from the JunkWaxData Github Repository
        /// </summary>
        /// <param name="year"></param>
        /// <param name="setName"></param>
        public bool LoadSet(string setLabel)
        {
            if (_cardLists.ContainsKey(setLabel))
                return true;

            //Parse the Set Label into Year and Set Name
            var parts = setLabel.Split("|");
            var year = parts[0];
            var setName = parts[1];

            //URL Format for GitHub JSON Files is: {GitHubBaseUrl}/{Year}/{Year}-{SetName}.json
            var url = $"{appSettings.Value.CardListBaseUrl}/{year}/{year}-{setName}.json";

            //Download the Card List JSON File
            var cardListJson = httpClient.GetStringAsync(url).Result;

            //Deserialize the JSON into a CardList Object
            var cardList = JsonSerializer.Deserialize<CardList>(cardListJson);

            //Save the Card List to the Dictionary
            if(cardList != null)
                _cardLists[setLabel] = cardList;

            return true;
        }

        /// <summary>
        ///     Searches through the specified set for the player name extracted from the baseball card
        /// </summary>
        /// <param name="setLabel"></param>
        /// <param name="playerString"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool CardSearch(string setLabel, string playerString, out CardSearchResult cardSearchResult)
        {
            cardSearchResult = new(); //Assign a default card to return if we don't find the player
            var cleanedPlayerString = CleanupString(playerString);

            //Player names will have at least one space -- if there isn't one, bail.
            if (!cleanedPlayerString.Trim().Contains(' '))
                return false;

            //If we don't have the set, load it
            if (!_cardLists.ContainsKey(setLabel))
            {
                if (!LoadSet(setLabel))
                    return false;
            }

            var setToSearch = _cardLists[setLabel];

            //Search through all sets for a card with name containing the player string
            foreach (var set in setToSearch.Sets)
            {
                //Find Exact Matches
                foreach (var c in set.Cards)
                {
                    if (!c.Name.Equals(cleanedPlayerString, StringComparison.InvariantCultureIgnoreCase)) 
                        continue;

                    cardSearchResult.Match = MatchType.Exact;
                    cardSearchResult.Card = c;
                    return true;
                }

                //Find Partial Matches (not great, but at this point we're just trying to find the closest match)
                foreach (var c in set.Cards)
                {
                    if (!c.Name.StartsWith(cleanedPlayerString, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    cardSearchResult.Match = MatchType.Exact;
                    cardSearchResult.Card = c;
                    return true;
                }
            }

            return false;

        }

        /// <summary>
        ///     Cleans up the possible player name by removing common strings (such as position, acronyms, etc.)
        ///     from the string that OCR might read from the card.
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public string CleanupString(string playerName)
        {
            //Setup a working name to trim and filter
            var workingName = playerName.Trim();

            //Bail here if it was just whitespace
            if (string.IsNullOrWhiteSpace(workingName))
                return workingName;

            //Filter out any characters after a found period (for players like Jr., Sr., etc.)
            var periodIndex = workingName.IndexOf('.');
            if (periodIndex > 0)
                workingName = workingName.Substring(0, periodIndex);

            // This pattern matches one or more occurrences of:
            //   - A whitespace, followed by
            //   - A whole word that matches one of the known position tokens
            //     (both abbreviations and full-word versions, including those with spaces, as well as common OCR misses),
            //   - Followed by any trailing whitespace until the end of the string.
            var pattern = @"(\s+\b(?:2B|28|1B|18|3B|38|SS|LF|CF|RF|C|P|Pitcher|Catcher|First\s+Base|Second\s+Base|Third\s+Base|Shortstop|Left\s+Field|Center\s+Field|Right\s+Field)\b)+\s*$";

            // Remove any trailing position tokens (ignoring case) from the player name.
            return Regex.Replace(workingName, pattern, string.Empty, RegexOptions.IgnoreCase);
        }

    }
}
