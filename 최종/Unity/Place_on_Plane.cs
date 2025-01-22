using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

    [SerializeField]
    GameObject visualObject;

    [SerializeField]
    ARPlaneManager arPlaneManager; // AR Plane Manager 추가

    UnityEvent placementUpdate;

    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    public GameObject spawnedObject { get; private set; }

    ARRaycastManager m_RaycastManager;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();

        if (placementUpdate == null)
            placementUpdate = new UnityEvent();

        placementUpdate.AddListener(DiableVisual);

        // Visual Object의 크기를 절반으로 줄이기
        if (visualObject != null)
        {
            visualObject.transform.localScale *= 0.5f; // 크기를 절반으로 조정
        }
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }

        touchPosition = default;
        return false;
    }

    void Update()
    {
        if (!TryGetTouchPosition(out Vector2 touchPosition))
            return;

        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = s_Hits[0].pose;

            if (spawnedObject == null)
            {
                // Instantiate the object and set its rotation
                spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, Quaternion.Euler(90, 0, 0)); 

                // Placed Prefab의 크기를 절반으로 줄이기
                spawnedObject.transform.localScale *= 0.5f;

                // 평면 숨기기
                HideAllPlanes();
            }
            else
            {
                spawnedObject.transform.position = hitPose.position;
            }

            placementUpdate.Invoke();
        }
    }

    public void DiableVisual()
    {
        visualObject.SetActive(false);
    }

    private void HideAllPlanes()
    {
        // 평면을 모두 숨기고 새로운 평면 감지를 중지
        foreach (ARPlane plane in arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(false); // 평면 비활성화
        }

        arPlaneManager.enabled = false; // 새로운 평면 감지 중지
    }
}
