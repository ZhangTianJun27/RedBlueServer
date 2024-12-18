using RedBlue_Server.System;

namespace RedBlue_Server.Server;

public class ServerRoot : SingletonBase<ServerRoot>
{
    public override void Init()
    {
        base.Init();
        Tools.Instance.Init();
        ServerManager.Instance.Init();
        
        
        RoomSys.Instance.Init();
        DataSys.Instance.Init();
        GeneralSys.Instance.Init();
        WeaponSys.Instance.Init();
        PlayerSys.Instance.Init();
        MatchSys.Instance.Init();
    }

    public override void Update()
    {
        base.Update();
        ServerManager.Instance.Update();
        
        
        RoomSys.Instance.Update();
        DataSys.Instance.Update();
        GeneralSys.Instance.Update();
        WeaponSys.Instance.Update();
        PlayerSys.Instance.Update();
        MatchSys.Instance.Update();
    }
}