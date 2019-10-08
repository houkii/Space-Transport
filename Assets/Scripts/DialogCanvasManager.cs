using UnityEngine;

public class DialogCanvasManager : Singleton<DialogCanvasManager>
{
    public InfoDialog Info;
    public OvelappingCanvas overlapping;
    public MiddleInfo midInfo;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Info.Show("TEST", "TEST", null);
        }
    }
}