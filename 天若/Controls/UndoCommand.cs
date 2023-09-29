using System.Collections.Generic;

namespace TrOCR.Controls;

public class UndoCommand
{
    public string Record { get; private set; }
    private List<string> undoList = new( );
    private List<string> redoList = new( );
    private int undoCount = -1;
    private bool CanUndo;

    public UndoCommand(int UndoCount)
    {
        undoCount = UndoCount + 1;
        undoList.Add("");
    }

    public void Execute(string command)
    {
        Record = command;
        if (!CanUndo)
        {
            undoList.Add(command);
            if (undoCount == -1 || undoList.Count <= undoCount)
                return;
            undoList.RemoveAt(0);
        }
        else
        {
            CanUndo = false;
        }
    }

    public void Undo( )
    {
        if (undoList.Count <= 1)
            return;
        CanUndo = true;
        redoList.Add(undoList[undoList.Count - 1]);
        undoList.RemoveAt(undoList.Count - 1);
        Record = undoList[undoList.Count - 1];
    }

    public void Redo( )
    {
        if (redoList.Count > 0)
        {
            Record = redoList[redoList.Count - 1];
            redoList.RemoveAt(redoList.Count - 1);
        }
    }
}
