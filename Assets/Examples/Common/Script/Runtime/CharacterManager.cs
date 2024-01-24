using System.Collections.Generic;
using UnityEngine;
using System;

// Profilerの値を変化を見てもらうため、
// キャラクターを出したり引っ込めたりする処理を書いています
public class CharacterManager : MonoBehaviour
{
    // 生成するPrefab
    [SerializeField]
    private GameObject prefab;
    // PrefabからInstantiateしたGameObject一覧
    private List<GameObject> gameObjects = new List<GameObject>();

    // Singletonのような感じで取得する用
    private static CharacterManager s_instance;
    public static CharacterManager Instance
    {
        get
        {
            return s_instance;
        }
    }
    // キャラクター数が変わったときに呼び出すActionを外部から登録します
    public Action<int> OnCharacterNumberChange;

    // Awake処理
    private void Awake()
    {
        // 初期に一体は配置
        OnAddCharacter();
        s_instance = this;
    }

    // キャラクター追加時の処理
    public void OnAddCharacter()
    {
        var position = new Vector3(-0.7f,0.0f, gameObjects.Count * -0.5f);

        GameObject character = GameObject.Instantiate(prefab,position,Quaternion.identity);
        gameObjects.Add(character);
        if (OnCharacterNumberChange != null)
        {
            OnCharacterNumberChange(gameObjects.Count);
        }
    }
    // キャラクター削除時の処理
    public void OnRemoveCharacter()
    {
        if (gameObjects.Count > 0)
        {
            var character = gameObjects[gameObjects.Count - 1];
            gameObjects.RemoveAt(gameObjects.Count - 1);
            GameObject.Destroy(character);
        }
        if (OnCharacterNumberChange != null)
        {
            OnCharacterNumberChange(gameObjects.Count);
        }
    }
    // 破棄時の処理
    private void OnDestroy()
    {
        s_instance = null;
    }
}
