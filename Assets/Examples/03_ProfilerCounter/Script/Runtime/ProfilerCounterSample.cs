using Unity.Profiling;
using UnityEngine;

// ProfilerCounterによる任意の値のカウントするサンプルです
public class ProfilerCounterSample : MonoBehaviour
{
    // Generic型でカウンターを宣言します
    static readonly ProfilerCounter<int> characterNumber =
       new ProfilerCounter<int>(ProfilerCategory.Scripts,
           "キャラの数",
           ProfilerMarkerDataUnit.Count);
    // 現在表示のキャラクター数
    private int characterNum;

    // キャラクター数が変わったときに呼び出されます
    public void OnCharacterNumChanged(int number)
    {
        this.characterNum = number;
    }

    // Start時の処理
    void Start()
    {
        // Start時にCharacterManagerのキャラ数変更を登録します
        CharacterManager.Instance.OnCharacterNumberChange += OnCharacterNumChanged;
    }

    // Update処理
    void Update()
    {
        // カウンター値として、キャラクター数を表示します
        characterNumber.Sample(characterNum);
    }
}
