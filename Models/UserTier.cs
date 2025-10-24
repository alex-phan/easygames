// A shared enum to access user tiers across the application. This lets us easily add, edit or delete tiers in one place. (Alex Phan) 

namespace EasyGames.Models
{
    public enum UserTier
    {
        Bronze,
        Silver,
        Gold,
        Platinum
    }
}
