namespace DevelopersHub.ClashOfWhatecer
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class UI_RequiredBuilding : MonoBehaviour
    {

        [SerializeField] private Image _Icon = null;
        [SerializeField] public TextMeshProUGUI _title = null;
        [SerializeField] public TextMeshProUGUI _count = null;

        public void Initialize(Data.BuildingID id, int count)
        {
            _title.text = Language.instance.GetBuildingName(id);
            Sprite icon = AssetsBank.GetBuildingIcon(id);
            if (icon != null)
            {
                _Icon.sprite = icon;
            }
            _count.text = "x" + count.ToString();
            _title.ForceMeshUpdate(true);
            _count.ForceMeshUpdate(true);
        }

    }
}