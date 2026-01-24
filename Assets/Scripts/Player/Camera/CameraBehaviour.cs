using UnityEngine;
using System.Collections;

public static class CameraBehaviour
{

    public static void LookTarget(Camera cam, Transform target)
    {
        if (cam != null && target != null)
            cam.transform.LookAt(target.position);
    }

    public static void CamPositionOffset(Camera cam, Transform target, Vector3 offset)
    {
        if (cam != null && target != null)
            cam.transform.position = target.position + offset;
    }

    public static void CamRotation(Camera cam, Quaternion CamQuaternion)
    {
        if (cam != null && CamQuaternion != null)
            cam.transform.rotation = CamQuaternion;
    }

    public static void CamFOV(Camera cam, float targetFov, float zoomSpeed)
    {
        if (cam != null)
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
    }
    
    public static void TargetTransition(Transform target, Vector3 newPosition, float speed)
    {
        if (target != null)
            target.position = Vector3.Lerp(target.position, newPosition, Time.deltaTime * speed);
    }

    public static void MoveCamOffset(Camera cam, Transform target, Vector3 offset)
    {
        if (cam != null && target != null)
        {
            cam.transform.position = target.position + offset;
            cam.transform.LookAt(target.position);
        }
    }

    public static IEnumerator HitPause(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    public static IEnumerator CameraShake(Camera cam, float magnitude, float duration)
    {
        if (cam == null) yield break;
        
        Vector3 originalPosition = cam.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cam.transform.position = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.transform.position = originalPosition;
    }

}
