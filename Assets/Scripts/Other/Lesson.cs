using UnityEngine;

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