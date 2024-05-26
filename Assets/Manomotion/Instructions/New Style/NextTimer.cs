using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NextTimer : MonoBehaviour
{
    [SerializeField] float secondsBeforeChange = 3f;
    [SerializeField] Image timerImage;
    [SerializeField] UnityEvent OnTimerFinished;

    float timeSpent = 0;

    private void OnEnable()
    {
        timeSpent = 0;
        timerImage.fillAmount = 0;
    }

    private void Update()
    {
        timeSpent += Time.deltaTime;
        timerImage.fillAmount = timeSpent / secondsBeforeChange;

        if (timeSpent > secondsBeforeChange)
        {
            gameObject.SetActive(false);
            OnTimerFinished?.Invoke();
        }
    }
}