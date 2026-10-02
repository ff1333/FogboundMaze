namespace FogboundMaze
{
    public enum WeaponType
    {
        None,
        Pistol,
        Machete
    }

    public enum GamePhase
    {
        Staging,
        Playing,
        Paused,
        Won,
        Lost
    }

    public enum EnemyState
    {
        Inactive,
        Wander,
        Chase,
        Attack,
        Dead
    }
}

