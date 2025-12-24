
public class CoinPickup : Pickup
{
    PlayerCollector collector;
    public int coins = 1;


    protected override void OnDestroy()
    {
        base.OnDestroy();
        //retrieve the PlayerCollector component from player who picked up this object
        //add coins to their total
        if (target != null)
        {
            collector = target.GetComponentInChildren<PlayerCollector>();
            if(collector != null) collector.AddCoins(coins);
        }
    }
}
