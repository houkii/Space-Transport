using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionData", menuName = "Mission", order = 1)]
public class Mission : ScriptableObject
{
    public string Name;
    public Vector3 PlayerPosition;
    public List<PlanetarySystemInstance> PlanetarySystems;
    public List<PlanetInstance> Planets;
    public List<TravellerInstance> NpcsToSpawn;
    public int BoundsSize;
    public Tutorial tutorial;

    public void Initialize()
    {
        tutorial.OnTutorialCompleted += SaveToPrefs;
        if (PlayerPrefs.HasKey(TutorialString))
        {
            var tutorialCompleteInt = PlayerPrefs.GetInt(TutorialString);
            if (tutorialCompleteInt == 1)
                tutorial.Complete = true;
        }
    }

    private void SaveToPrefs()
    {
        PlayerPrefs.SetInt(Name + "Tutorial", 1);
    }

    private string TutorialString => Name + "Tutorial";
}

[System.Serializable]
public class Tutorial
{
    [SerializeField] private List<Lesson> lessons;
    public bool HasLessons => lessons.Count > 0;
    public bool Complete { get; set; }

    public Action OnTutorialCompleted;

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

[System.Serializable]
public class Lesson
{
    [SerializeField] private string title;
    [SerializeField] private string info;

    public string Title => title;
    public string Info => GetInfoString();

    public Lesson(string title, string info)
    {
        this.title = title;
        this.info = info;
    }

    private string GetInfoString()
    {
        return info.Replace("@", System.Environment.NewLine);
    }
}