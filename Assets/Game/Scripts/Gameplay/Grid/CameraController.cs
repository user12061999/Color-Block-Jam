using System;
using UnityEngine;

[ExecuteAlways]
public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GridBoundsCalculator gridBoundsCalculator;
    [SerializeField] private Vector2 widthRange = new Vector2(0.1f, 0.9f);
    [SerializeField] private Vector2 heightRange = new Vector2(0.1f, 0.9f);

    [SerializeField] private float fieldOfView = 60f; // Góc Field of View

    public float FieldOfView
    {
        get => fieldOfView;
        set
        {
            fieldOfView = value;
            if (!mainCamera.orthographic)
            {
                mainCamera.fieldOfView = fieldOfView;
            }
        }
    }

    private void Start()
    {
        UpdateCamera(gridBoundsCalculator.gridBounds);
    }

    private void OnValidate()
    {
        mainCamera.fieldOfView = fieldOfView; // Áp dụng giá trị FoV khi validate
        UpdateCamera(gridBoundsCalculator.gridBounds);
    }

    public void UpdateCamera(Bounds bounds)
    {
        Vector2 heightRange = this.heightRange;

        float distance = Mathf.Abs(mainCamera.transform.localPosition.z);
        float boundsAspect = bounds.size.x / bounds.size.y;
        float cameraAspect = mainCamera.aspect;
        float limitCameraAspect = cameraAspect * ((widthRange.y - widthRange.x) / (heightRange.y - heightRange.x));

        float cameraWidth = 0;
        float cameraHeight = 0;

        float widthPercent = 1f - widthRange.x - (1f - widthRange.y);
        float heightPercent = 1f - heightRange.x - (1f - heightRange.y);

        if (boundsAspect > limitCameraAspect)
        {
            cameraWidth = bounds.size.x / widthPercent;
            cameraHeight = cameraWidth / cameraAspect;
        }
        else
        {
            cameraHeight = bounds.size.y / heightPercent;
            cameraWidth = cameraHeight * cameraAspect;
        }

        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize = cameraHeight / 2;

            float left = (-0.5f + widthRange.x) * cameraWidth;
            float right = (widthRange.y - 0.5f) * cameraWidth;
            float top = (heightRange.y - 0.5f) * cameraHeight;
            float bottom = (-0.5f + heightRange.x) * cameraHeight;

            Vector3 offset = new Vector3((left + right) / 2f, (top + bottom) / 2f, 0.5f);
            transform.position = bounds.center - offset;
        }
        else
        {
            mainCamera.fieldOfView = fieldOfView; // Sử dụng giá trị FoV từ đầu vào
            fieldOfView = 2 * Mathf.Atan(cameraHeight / (2 * distance)) * Mathf.Rad2Deg;
            mainCamera.fieldOfView = fieldOfView;

            Vector3 offset = new Vector3(0, 0, 0.5f);
            transform.position = bounds.center - offset;
        }
    }
}