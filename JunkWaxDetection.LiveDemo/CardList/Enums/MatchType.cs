namespace JunkWaxDetection.LiveDemo.CardList.Enums
{
    public enum MatchType
    {
        /// <summary>
        ///     No Match Found/No Match Performed
        /// </summary>
        None,

        /// <summary>
        ///     Exact Match of Player Name to Card in Set
        /// </summary>
        Exact,

        /// <summary>
        ///     Partial Match of Player Name to Card in Set, Card Name Starts with search string
        /// </summary>
        StartsWith
    }
}
