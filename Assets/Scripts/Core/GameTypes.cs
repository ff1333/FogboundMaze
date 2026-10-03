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
        Title,
        Guide,
        LevelSelect,
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
