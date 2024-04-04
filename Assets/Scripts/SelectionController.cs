using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionController : MonoBehaviour
{
    private Camera _mainCamera;

    public bool IsBaseSelected { get; private set; } = false;

    public GameObject LastClickedObject { get; private set; }

    public Ray Ray { get; private set; }

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        Ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
    }

    public void TrySelectBase(LayerMask layerMask)
    {
        if (Physics.Raycast(Ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            SetLastClickedObject(hit.collider.gameObject);

            if (hit.collider.gameObject.TryGetComponent(out Base goldBase))
            {
                if (Input.GetMouseButtonDown(0) && LastClickedObject == gameObject)
                {
                    IsBaseSelected = true;
                    return;
                }
            }
        }

        if (Input.GetMouseButtonDown(0) && IsBaseSelected == true)
        {
            IsBaseSelected = false;
        }
    }

    private void SetLastClickedObject(GameObject gameObject)
    {
        if (Input.GetMouseButtonDown(0))
        {
            LastClickedObject = gameObject;
        }
    }
}
