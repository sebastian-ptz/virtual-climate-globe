using UnityEngine;
using UnityEngine.UIElements;

public class TextureInterface : MonoBehaviour
{
    [SerializeField] public UIDocument uiDocument;

    private void OnEnable()
    {
        VisualElement root = uiDocument.rootVisualElement;
        var textureListController = new TextureController();
        textureListController.InitializeTextureList(root);
    }
}
