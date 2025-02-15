using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JunkWaxDetection.LiveDemo.CardList
{
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

        public (bool foundPlayer, Card card) HasPlayer(string setLabel, string playerString)
        {
            var cleanedPlayerString = CleanupString(playerString);

            //If we don't have the set, load it
            if (!_cardLists.ContainsKey(setLabel))
            {
                if (!LoadSet(setLabel))
                    return (false, new Card());
            }

            var setToSearch = _cardLists[setLabel];

            //Search through all sets for a card with name containing the player string
            foreach (var set in setToSearch.Sets)
            {
                //Find Exact Matches
                foreach (var card in set.Cards)
                {
                    if (card.Name.Equals(cleanedPlayerString, StringComparison.InvariantCultureIgnoreCase))
                        return (true, card);
                }

                //Find Partial Matches
                foreach (var card in set.Cards)
                {
                    if (card.Name.StartsWith(cleanedPlayerString, StringComparison.InvariantCultureIgnoreCase))
                        return (true, card);
                }
            }

            return (false, new Card());

        }

        /// <summary>
        ///     Cleans up the possible player name by removing common strings (such as position, acronyms, etc.)
        ///     from the string that OCR might read from the card.
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public string CleanupString(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return playerName;

            //Filter out any characters after a found period (for players like Jr., Sr., etc.)
            var periodIndex = playerName.IndexOf('.');
            if (periodIndex > 0)
                playerName = playerName.Substring(0, periodIndex);

            // This pattern matches one or more occurrences of:
            //   - A whitespace, followed by
            //   - A whole word that matches one of the known position tokens
            //     (both abbreviations and full-word versions, including those with spaces, as well as common OCR misses),
            //   - Followed by any trailing whitespace until the end of the string.
            var pattern = @"(\s+\b(?:2B|28|1B|18|3B|38|SS|LF|CF|RF|C|P|Pitcher|Catcher|First\s+Base|Second\s+Base|Third\s+Base|Shortstop|Left\s+Field|Center\s+Field|Right\s+Field)\b)+\s*$";

            // Remove any trailing position tokens (ignoring case) from the player name.
            return Regex.Replace(playerName, pattern, string.Empty, RegexOptions.IgnoreCase);
        }

    }
}
