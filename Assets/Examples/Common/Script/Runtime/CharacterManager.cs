using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterManager : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;

    private List<GameObject> characters = new List<GameObject>();

    private static CharacterManager s_instance;
    public static CharacterManager Instance
    {
        get
        {
            return s_instance;
        }
    }

    public Action<int> OnCharacterNumberChange;
    private void Awake()
    {
        // 初期に一体は配置
        OnAddCharacter();
        s_instance = this;
    }

    public void OnAddCharacter()
    {
        var position = new Vector3(-0.7f,0.0f, characters.Count * -0.5f);

        GameObject character = GameObject.Instantiate(prefab,position,Quaternion.identity);
        characters.Add(character);
        if (OnCharacterNumberChange != null)
        {
            OnCharacterNumberChange(characters.Count);
        }
    }
    public void OnRemoveCharacter()
    {
        if (characters.Count > 0)
        {
            var character = characters[characters.Count - 1];
            characters.RemoveAt(characters.Count - 1);
            GameObject.Destroy(character);
        }
        if (OnCharacterNumberChange != null)
        {
            OnCharacterNumberChange(characters.Count);
        }
    }
    private void OnDestroy()
    {
        s_instance = null;
    }
}
