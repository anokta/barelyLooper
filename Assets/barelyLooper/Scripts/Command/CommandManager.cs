using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandManager {
  
  private Stack<Command> undos;
  private Stack<Command> redos;

  private bool IsUndoAvailable {
    get { return undos.Count > 0; }
  }

  private bool IsRedoAvailable {
    get { return redos.Count > 0; }
  }

  public CommandManager() {
    undos = new Stack<Command>();
    redos = new Stack<Command>();
  }

  public void ExecuteCommand(Command command) {
    command.Execute();
    undos.Push(command);
    redos.Clear();
  }

  public void Undo() {
    if (IsUndoAvailable) {
      Command command = undos.Pop();
      command.Undo();
      redos.Push(command);
    }
  }

  public void Redo() {
    if (IsRedoAvailable) {
      Command command = redos.Pop();
      command.Execute();
      undos.Push(command);
    }
  }

}
