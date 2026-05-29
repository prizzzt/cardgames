namespace CardGames.Entities.Scoundrel
{
    public class Dungeon
    {
        public GameState State { get; private set; }
        public Queue<Card> Deck { get; }

        public Card?[] Room { get; }
        public const int RoomSize = 4;
        public bool RoomSkipped { get; private set; }

        public Card? CurrentWeapon { get; private set; }
        public Card? LastKilledMonster { get; private set; }
        public bool BareHands { get; set; }

        public int Health { get; private set; }
        public const int HealthMax = 20;

        public Dungeon()
        {
            State = GameState.Continues;
            Deck = new Queue<Card>();

            Room = new Card?[RoomSize];
            RoomSkipped = false;

            CurrentWeapon = null;
            LastKilledMonster = null;
            BareHands = false;

            Health = HealthMax;

            var suits = Enum.GetValues<Suit>();
            var cards = Enum.GetValues<Rank>()
                .SelectMany(r => suits.Select(s => new Card(s, r)))
                .Where(c => c.Suit == Suit.Spades || c.Suit == Suit.Clubs || c.Rank < Rank.Jack)
                .Shuffle();
            foreach (var card in cards)
                Deck.Enqueue(card);

            TryRepopulateRoom();
        }

        public bool Action(int cardIndex)
        {
            if (State != GameState.Continues || cardIndex >= RoomSize)
                return false;

            var card = Room[cardIndex];
            if (card == null)
                return false;

            var result = card.Suit switch
            {
                Suit.Hearts => Heal(card),
                Suit.Diamonds => Equip(card),
                Suit.Spades => Fight(card),
                Suit.Clubs => Fight(card),
                _ => false
            };

            if (result)
            {
                Room[cardIndex] = null;
                TryRepopulateRoom();
            }

            return result;
        }

        public bool SkipRoom()
        {
            if (CanSkipRoom())
            {
                for (var i = 0; i < RoomSize; i++)
                {
                    if (Room[i] != null)
                    {
                        Deck.Enqueue(Room[i]!);
                        Room[i] = null;
                    }
                }

                TryRepopulateRoom();
                RoomSkipped = true;
                return true;
            }

            return false;
        }

        public bool CanSkipRoom()
        {
            if (State != GameState.Continues || RoomSkipped)
                return false;

            foreach (var card in Room)
                if (card == null)
                    return false;

            return true;
        }

        private bool Heal(Card card)
        {
            Health += (int)card.Rank;
            if (Health > HealthMax)
                Health = HealthMax;
            return true;
        }

        private bool Equip(Card card)
        {
            CurrentWeapon = card;
            LastKilledMonster = null;
            return true;
        }

        private bool Fight(Card card)
        {
            if (BareHands || CurrentWeapon == null)
            {
                Health -= (int)card.Rank;
            }
            else
            {
                if (LastKilledMonster != null && card.Rank >= LastKilledMonster.Rank)
                    return false;

                if (CurrentWeapon.Rank < card.Rank)
                    Health -= card.Rank - CurrentWeapon.Rank;

                LastKilledMonster = card;
            }

            if (Health <= 0)
                State = GameState.Defeat;

            return true;
        }

        private void TryRepopulateRoom()
        {
            if (State != GameState.Continues)
                return;

            bool hasCard = false;
            foreach (var card in Room)
                if (card != null)
                    if (hasCard)
                        return;
                    else
                        hasCard = true;

            for (var i = 0; i < RoomSize; i++)
            {
                if (Room[i] == null)
                {
                    if (Deck.TryDequeue(out var card))
                        Room[i] = card;
                    else
                    {
                        State = GameState.Win;
                        return;
                    }
                }
            }
            RoomSkipped = false;
        }
    }
}
