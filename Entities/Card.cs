namespace CardGames.Entities
{
    public class Card(Suit suit, Rank rank)
    {
        public Suit Suit { get; set; } = suit;
        public Rank Rank { get; set; } = rank;
    }
}
