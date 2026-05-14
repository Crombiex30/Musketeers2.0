using UnityEngine;
using System.Collections;

public class bridgerotation : MonoBehaviour
{
    [SerializeField] private GameObject objectToRotate;
    [SerializeField] private float rotationAmount = 90f;
    [SerializeField] private float rotationDuration = 1f;

    private bool isRotating = false;
    private bool hasRotated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isRotating && !hasRotated)
        {
            StartCoroutine(RotateObject());
        }
    }

    private IEnumerator RotateObject()
    {
        isRotating = true;

        Quaternion startRotation = objectToRotate.transform.rotation;

        Quaternion endRotation = startRotation * Quaternion.Euler(0f, 0f, rotationAmount);

        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            objectToRotate.transform.rotation = Quaternion.Lerp(
                startRotation,
                endRotation,
                elapsedTime / rotationDuration
            );

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        objectToRotate.transform.rotation = endRotation;

        isRotating = false;
        hasRotated = true;
    }
}