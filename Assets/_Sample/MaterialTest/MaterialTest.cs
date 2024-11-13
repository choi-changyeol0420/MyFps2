using UnityEngine;

namespace MySample
{
    public class MaterialTest : MonoBehaviour
    {
        #region Variables
        private Renderer _renderer;

        private MaterialPropertyBlock propertyBlock;
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            _renderer = GetComponent<Renderer>();
            //_renderer.material.SetColor("_BaseColor",Color.red);
            //_renderer.sharedMaterial.SetColor("_BaseColor", Color.red);

            propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_BaseColor", Color.red);
            _renderer.SetPropertyBlock(propertyBlock);
        }

    }
}