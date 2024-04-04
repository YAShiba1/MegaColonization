using System.Collections.Generic;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    private Queue<Transform> _goldsPoints;

    private void Start()
    {
        _goldsPoints = new Queue<Transform>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Gold gold))
        {
            _goldsPoints.Enqueue(gold.transform);
        }
    }

    public Transform TryGetNextGold()
    {
        if (_goldsPoints.Count > 0)
        {
            return _goldsPoints.Dequeue();
        }

        return null;
    }
}
