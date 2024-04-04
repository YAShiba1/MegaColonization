using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Base : MonoBehaviour
{
    [SerializeField] private Transform _botSpawnPoint;
    [SerializeField] private BotSpawner _botSpawner;
    [SerializeField] private Flag _flagPrefab;
    [SerializeField] private Scanner _scanner;
    [SerializeField] private Base _basePrefab;
    [SerializeField] private int _maximumNumberOfBots;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private SelectionController _selectionController;

    private List<Bot> _bots;
    private Queue<Transform> _scanGoldsPoints;
    private Dictionary<Transform, bool> _freeGoldsPoints;
    private Flag _flag;

    public Vector3 CoordinateForNewBase { get; private set; }

    public bool IsCoordinateSetted { get; private set; } = false;

    public int GoldCount { get; private set; } = 0;

    public bool IsFlagSpawned { get; private set; } = false;

    public Bot BotForNewColonyBase { get; private set; }

    public event UnityAction<int> GoldChanged;

    private void Awake()
    {
        _bots = new List<Bot>();
        _scanGoldsPoints = new Queue<Transform>();
        _freeGoldsPoints = new Dictionary<Transform, bool>();

        _botSpawner.SpawnStarterBots(_bots);
    }

    private void Update()
    {
        ScanArea();
        TrySpawnFlag();
        _selectionController.TrySelectBase(_layerMask);
        SetCoordinateForNewBase();
        TryBuyNewBot();
    }

    public void TakeGold()
    {
        GoldCount++;
        GoldChanged?.Invoke(GoldCount);
    }

    public void Pay(int price)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price));
        }

        if (GoldCount >= price)
        {
            GoldCount -= price;
            GoldChanged?.Invoke(GoldCount);
        }
    }

    public void AddBot(Bot givenBot)
    {
        if (givenBot != null)
        {
            _bots.Add(givenBot);
        }
    }

    public Bot GetBotForNewBase()
    {
        if (BotForNewColonyBase != null && IsCoordinateSetted == true)
        {
            Bot bot = BotForNewColonyBase;

            BotForNewColonyBase = null;
            IsCoordinateSetted = false;

            return bot;
        }

        return null;
    }

    public void TrySendFreeBotToFlag()
    {
        if (IsFlagSpawned == true)
        {
            BotForNewColonyBase = TryGetFreeBot();

            if (BotForNewColonyBase != null && _flag != null)
            {
                _bots.Remove(BotForNewColonyBase);

                BotForNewColonyBase.SetTarget(_flag.transform);
            }
        }
    }

    public void AddGoldPoint(Transform filtredGoldPoint)
    {
        if (filtredGoldPoint != null && !_freeGoldsPoints.ContainsKey(filtredGoldPoint))
        {
            bool isPositionReserved = false;

            _freeGoldsPoints.Add(filtredGoldPoint, isPositionReserved);
        }
    }

    public Transform GetScanGoldPoint()
    {
        if (_scanGoldsPoints.Count > 0)
        {
            return _scanGoldsPoints.Dequeue();
        }

        return null;
    }

    private void TryBuyNewBot()
    {
        int botPrice = 3;

        if (IsFlagSpawned == false && GoldCount >= botPrice && _bots.Count <= _maximumNumberOfBots - 1)
        {
            _botSpawner.Spawn(_bots);

            GoldCount -= botPrice;
            GoldChanged?.Invoke(GoldCount);
        }
    }

    private void ScanArea()
    {
        Transform scanGoldPoint = _scanner.TryGetNextGold();

        if (scanGoldPoint != null)
        {
            _scanGoldsPoints.Enqueue(scanGoldPoint);
        }

        if(_freeGoldsPoints.Count > 0)
        {
            TryToSendFreeBotForGold();
        }
    }

    private void TryToSendFreeBotForGold()
    {
        Bot freeBot = TryGetFreeBot();

        if(freeBot != null && _freeGoldsPoints.Count > 0)
        {
            foreach (var freeGoldPosition in _freeGoldsPoints)
            {
                if (freeGoldPosition.Value == false)
                {
                    freeBot.SetTarget(freeGoldPosition.Key);
                    _freeGoldsPoints[freeGoldPosition.Key] = true;

                    return;
                }
            }
        }
    }

    private Bot TryGetFreeBot()
    {
        Bot freeBot = null;

        if (_bots != null && _bots.Count > 0)
        {
            foreach (Bot bot in _bots)
            {
                if (bot.CurrentTarget == null)
                {
                    freeBot = bot;

                    break;
                }
            }
        }

        return freeBot;
    }

    private Flag CreateFlag(Vector3 spawnPosition)
    {
        Flag flag = Instantiate(_flagPrefab, spawnPosition, Quaternion.identity);

        return flag;
    }

    private void TrySpawnFlag()
    {
        int minimumNumberOfBaseBots = 1;

        if (_selectionController.IsBaseSelected == true && Input.GetMouseButtonDown(0) && _bots.Count > minimumNumberOfBaseBots)
        {
            if (Physics.Raycast(_selectionController.Ray, out RaycastHit hit, Mathf.Infinity, _layerMask))
            {
                GameObject hitObject = hit.collider.gameObject;

                if (hitObject != null && hitObject.TryGetComponent(out Ground ground) && EventSystem.current.IsPointerOverGameObject() == false)
                {
                    Vector3 flagSpawnPosition = new Vector3(hit.point.x, _flagPrefab.transform.position.y, hit.point.z);

                    if (IsFlagSpawned == true && _flag != null)
                    {
                        Destroy(_flag.gameObject);

                        IsFlagSpawned = false;
                    }

                    _flag = CreateFlag(flagSpawnPosition);
                    IsFlagSpawned = true;
                }
            }
        }
    }

    private void SetCoordinateForNewBase()
    {
        if (BotForNewColonyBase != null && BotForNewColonyBase.IsFlagReached == true)
        {
            int distanceFromBotToNewBase = 3;

            Vector3 baseSpawnPosition = BotForNewColonyBase.transform.position;
            baseSpawnPosition.y = _basePrefab.transform.position.y;
            baseSpawnPosition.z += distanceFromBotToNewBase;

            CoordinateForNewBase = baseSpawnPosition;

            IsCoordinateSetted = true;
            IsFlagSpawned = false;
        }
    }
}
