using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tutorial
{
    public Action OnTutorialCompleted;
    public bool Complete { get; set; }
    public bool HasLessons => lessons.Count > 0;

    [SerializeField] private List<Lesson> lessons;
    private bool currentLessonViewed = false;

    public IEnumerator Show()
    {
        if (Complete)
            yield break;

        Complete = false;
        for (int i = 0; i < lessons.Count; ++i)
        {
            currentLessonViewed = false;
            ShowLesson(lessons[i]);
            yield return new WaitUntil(() => currentLessonViewed == true);
            yield return new WaitUntil(() => DialogCanvasManager.Instance.Info.gameObject.activeSelf == false);
            if (GameController.Instance.Settings.InfoActive == false)
                break;
        }
        Complete = true;
        OnTutorialCompleted?.Invoke();
        yield break;
    }

    private void ShowLesson(Lesson lesson)
    {
        DialogCanvasManager.Instance.Info.Show(lesson.Title, lesson.Info, () => currentLessonViewed = true);
    }
}