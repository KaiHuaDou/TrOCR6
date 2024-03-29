﻿using System.Collections.Generic;

namespace TrOCR.Controls;

public class UndoCommand
{
    private bool CanUndo;
    private List<string> redoList = new( );
    private int undoCount = -1;
    private List<string> undoList = new( );

    public UndoCommand(int UndoCount)
    {
        undoCount = UndoCount + 1;
        undoList.Add("");
    }

    public string Record { get; private set; }

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

    public void Redo( )
    {
        if (redoList.Count > 0)
        {
            Record = redoList[redoList.Count - 1];
            redoList.RemoveAt(redoList.Count - 1);
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
}
