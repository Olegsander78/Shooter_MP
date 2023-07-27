using Colyseus;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerManager : ColyseusManager<MultiplayerManager>
{
    [SerializeField]
    private GameObject _player;

    [SerializeField]
    private Transform _parent;

    [SerializeField]
    private EnemyMoveController _enemy;

    [SerializeField]
    private Transform _parentEnemy;

    private ColyseusRoom<State> _room;

    public void SendMessage(string key, Dictionary<string, object> data)
    {
        _room.Send(key, data);
    }

    protected override void Awake()
    {
        base.Awake();

        Instance.InitializeClient();
        Connect();
    }

    private async void Connect()
    {
        _room = await Instance.client.JoinOrCreate<State>("state_handler");

        _room.OnStateChange += OnChange;
    }

    private void OnChange(State state, bool isFirstState)
    {
        if (!isFirstState)
            return;

        state.players.ForEach((key, player) =>
        {
            if (key == _room.SessionId)
                CreatePlayer(player);
            else
                CreateEnemy(key, player);
        });

        _room.State.players.OnAdd += CreateEnemy;
        _room.State.players.OnRemove += RemoveEnemy;       
    }

    private void CreatePlayer(Player player)
    {
        var position = new Vector3(player.x, 0f, player.y);

        var playerPref = Instantiate(_player, position, Quaternion.identity, _parent);
        playerPref.gameObject.name = $"PlayerID_{_room.SessionId}";
    }

    private void CreateEnemy(string key, Player player)
    {
        var position = new Vector3(player.x, 0f, player.y);

        var enemy = Instantiate(_enemy, position, Quaternion.identity, _parentEnemy);

        enemy.gameObject.name = $"EnemyID_{key}";

        player.OnChange += enemy.OnChange;
    }


    private void RemoveEnemy(string key, Player player)
    {

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _room.Leave();
    }
}
