using UnityEngine;
using System.Collections;

// Command interface for undo/redo memory management.
public interface Command {
  // Executes the command.
  void Execute();

  // Undoes the command.
  void Undo();
}
