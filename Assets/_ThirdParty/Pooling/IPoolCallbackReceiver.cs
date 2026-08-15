// NY ROLLER RUSH - CORE SYSTEM
// Source: uPools (AnnulusGames)

namespace NYRollerRush.Pooling
{
    public interface IPoolCallbackReceiver
    {
        void OnRent();
        void OnReturn();
    }
}
