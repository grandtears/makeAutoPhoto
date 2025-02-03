using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class MaterialAndPlaneCreator : MonoBehaviour
{
    [MenuItem("Tools/Generate Planes and Materials")]
    public static void GeneratePlanesAndMaterials()
    {
        // フォルダパスの指定
        string imageFolderPath = "Assets/Images"; // Assetsフォルダ内の画像フォルダを指定
        Vector3 startPosition = Vector3.zero;     // Planeの初期位置
        float spacing = 1.0f;                    // Plane間のスペーシング
        int maxColumns = 20;                     // 1行に並べる最大数
        int currentColumn = 0;                   // 現在の列数（行のカウント用）

        // フォルダが存在するか確認
        if (!Directory.Exists(imageFolderPath))
        {
            Debug.LogError($"フォルダが見つかりません: {imageFolderPath}");
            return;
        }

        // フォルダ内の画像ファイルを取得
        string[] imageFiles = Directory.GetFiles(imageFolderPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(file => file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg"))
            .ToArray();

        if (imageFiles.Length == 0)
        {
            Debug.LogWarning("フォルダ内に画像ファイルが見つかりませんでした。");
            return;
        }

        Vector3 currentPosition = startPosition; // 配置位置

        foreach (string filePath in imageFiles)
        {
            // ファイル名を取得
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            // テクスチャを読み込む
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{imageFolderPath}/{Path.GetFileName(filePath)}");
            if (texture == null)
            {
                Debug.LogError($"テクスチャの読み込みに失敗しました: {filePath}");
                continue;
            }

            // マテリアルを作成
            Material material = new Material(Shader.Find("Standard"));
            material.mainTexture = texture;

            // マテリアルを保存
            string materialPath = $"{imageFolderPath}/{fileName}_Material.mat";
            AssetDatabase.CreateAsset(material, materialPath);

            // Planeを生成
            GameObject newPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            newPlane.transform.position = currentPosition;
            newPlane.name = $"Plane_{fileName}";

            // Planeにマテリアルを適用
            Renderer renderer = newPlane.GetComponent<Renderer>();
            renderer.material = material;

            // テクスチャのアスペクト比を取得
            float aspectRatio = (float)texture.width / texture.height;

            // Planeのサイズをアスペクト比に合わせて調整
            newPlane.transform.localScale = new Vector3(aspectRatio, 1, 1);

            // Planeの横幅（スケール x デフォルト幅10）を計算
            float planeWidth = newPlane.transform.localScale.x * 10;

            // 次のPlaneの配置位置を計算
            currentColumn++; // 1つ追加
            
            if (currentColumn >= maxColumns)
            {
                // 20枚配置したら改行（Z軸方向に移動）
                currentColumn = 0;
                currentPosition.x = startPosition.x; // X位置をリセット
                currentPosition.z -= 12; // 1行分下げる（10+余白2）
            }
            else
            {
                // 通常の配置（横方向）
                currentPosition.x += planeWidth + spacing;
            }
        }

        // アセットデータベースを更新
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Planeとマテリアルの生成が完了しました: {imageFiles.Length} 個のPlaneを生成しました。");
    }
}
