using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;

public class ProfilerCounterSample : MonoBehaviour
{
    static readonly ProfilerCounter<int> characterNumber =
       new ProfilerCounter<int>(ProfilerCategory.Scripts,
           "ƒLƒƒƒ‰‚Ì”",
           ProfilerMarkerDataUnit.Count);

    private int characterNum;

    public void OnCharacterNumChanged(int number)
    {
        this.characterNum = number;
    }

    void Start()
    {
        CharacterManager.Instance.OnCharacterNumberChange += OnCharacterNumChanged;
    }
    void Update()
    {
        characterNumber.Sample(characterNum);
    }
}
