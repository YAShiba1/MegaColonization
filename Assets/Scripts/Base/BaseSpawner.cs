using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseSpawner : MonoBehaviour
{
    [SerializeField] private Base _basePrefab;
    [SerializeField] private Transform _baseSpawnPoint;

    private List<Base> _bases;
    private Dictionary<Transform, bool> _filtredGoldsPoints;

    private const int NextIndexOffset = 1;

    private void Awake()
    {
        _filtredGoldsPoints = new Dictionary<Transform, bool>();

        _bases = new List<Base>
        {
            Spawn(_baseSpawnPoint.position)
        };
    }

    private void Update()
    {
        TryBuildNewColonyBase();
        TakeGoldPosition();
        GiveFiltredGoldPoint();
    }

    private void TryBuildNewColonyBase()
    {
        int newBasePrice = 5;

        if(_bases.Count > 0)
        {
            foreach(Base goldBase in _bases)
            {
                if (goldBase.GoldCount >= newBasePrice)
                {
                    if (goldBase.IsFlagSpawned == true && goldBase.BotForNewColonyBase == null)
                    {
                        goldBase.TrySendFreeBotToFlag();
                        goldBase.Pay(newBasePrice);
                    }
                }

                if(goldBase.IsCoordinateSetted == true)
                {
                    Bot givenBot = goldBase.GetBotForNewBase();
                    Base newBase = Spawn(goldBase.CoordinateForNewBase);

                    _bases.Add(newBase);
                    givenBot.SetParentBase(newBase);
                    newBase.AddBot(givenBot);

                    break;
                }
            }
        }
    }

    private Base Spawn(Vector3 baseSpawnPosition)
    {
        Base newBase = Instantiate(_basePrefab, baseSpawnPosition, Quaternion.identity);

        return newBase;
    }

    private void TakeGoldPosition()
    {
        if (_bases.Count > 0)
        {
            foreach (Base goldBase in _bases)
            {
                Transform goldPoint = goldBase.GetScanGoldPoint();

                if (goldPoint != null && _filtredGoldsPoints.ContainsKey(goldPoint) == false)
                {
                    bool isPositionProcessed = false;

                    _filtredGoldsPoints.Add(goldPoint, isPositionProcessed);
                }
            }
        }
    }

    private void GiveFiltredGoldPoint()
    {
        if (_filtredGoldsPoints.Count > 0 && _bases.Count > 0 && _filtredGoldsPoints.Count >= _bases.Count)
        {
            List<Transform> goldPositions = _filtredGoldsPoints.Keys.ToList();

            int baseIndex = 0;

            foreach (var goldPosition in goldPositions)
            {
                Base currentBase = _bases[baseIndex];

                if (_filtredGoldsPoints[goldPosition] == false)
                {
                    bool isPositionProcessed = true;

                    currentBase.AddGoldPoint(goldPosition);
                    _filtredGoldsPoints[goldPosition] = isPositionProcessed;
                }

                baseIndex = GetNextBaseIndex(baseIndex);
            }
        }
    }

    private int GetNextBaseIndex(int currentIndex)
    {
        if (currentIndex + NextIndexOffset < _bases.Count)
        {
            return currentIndex + NextIndexOffset;
        }
        else
        {
            return 0;
        }
    }
}
