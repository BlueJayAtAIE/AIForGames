using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Print : MonoBehaviour
{
    public IDecision decisionTreeRoot;
    
    IDecision currentDecision;

    // Toggle this on and off in-editor.
    public bool toggleMe = true;

    void Start()
    {
        decisionTreeRoot = new PrintDecision(toggleMe);
    }

    void Update()
    {
        // On update, set the current to the root.
        currentDecision = decisionTreeRoot;

        // keep going through the tree until you hit null. null will be at the end
        // of a final decision (like the leaf of a tree).
        while (currentDecision != null)
        {
            currentDecision = currentDecision.MakeDecision();
        }
    }
}

/// <summary>
/// An example Decision Tree. Prints yes or no depending on a given bool.
/// </summary>
public class PrintDecision : IDecision
{
    public bool branch = false;

    public PrintDecision() { }

    public PrintDecision(bool branch)
    {
        this.branch = branch;
    }

    public IDecision MakeDecision()
    {
        Debug.Log(branch ? "Yes" : "No");

        return null;
    }
}

// The Decision Tree on its own doesn't do anything. 
// It's just a part that can be used as a part of a larger system to let you mix and match different 
// decisions. You'll need to create a type that inherits from MonoBehaviour that you can use to 
// store a reference to an IDecision to evaluate.
