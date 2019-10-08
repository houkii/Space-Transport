using UnityEngine;

public class DialogCanvasManager : Singleton<DialogCanvasManager>
{
    public InfoDialog Info;
    public OvelappingCanvas overlapping;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Info.Show("TEST", "TEST", null);
        }
    }
}