using System.Collections;
using UnityEngine;

public class cameraShake : MonoBehaviour
{

    public bool start = false;
    public AnimationCurve curve;
    public float duration = 1f;

    private void Update()
    {
        if (start)
        {
            start = false;
            StartCoroutine(Shaking());
        }

    }

    IEnumerator Shaking()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / duration);
            transform.position = transform.position + Random.insideUnitSphere;
            yield return null;
        }
    }
}
