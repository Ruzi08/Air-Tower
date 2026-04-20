using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueEntry
{
    public string id;
    public string speaker;
    [TextArea(3, 5)] public string text;
    public float autoProgressDelay; // 0 = ждать клик, >0 = авто-продолжение через N сек
}

[Serializable]
public class DialogueCollection
{
    public List<DialogueEntry> dialogues;
}