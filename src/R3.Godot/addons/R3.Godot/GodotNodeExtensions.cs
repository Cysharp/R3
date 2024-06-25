using System;
using Godot;

namespace R3;

public static class GodotNodeExtensions
{
    /// <summary>
    /// Dispose self on target node has bee tree exited.
    /// </summary>
    /// <param name="disposable"></param>
    /// <param name="node"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Self disposable</returns>
    public static T AddTo<T>(this T disposable, Node node) where T : IDisposable
    {
        // Note: Dispose when tree exited, so if node is not inside tree, dispose immediately.
        if (!node.IsInsideTree()) 
        {
            if (!node.IsNodeReady()) // Before enter tree
            {
                GD.PrintErr("AddTo does not support to use before enter tree.");
            }

            disposable.Dispose();
            return disposable;
        }
        
        node.TreeExited += () => disposable.Dispose();
        return disposable;
    }
}
