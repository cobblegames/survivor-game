using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterChoiceBox : PopUpWindow
{
    private Image characterIcon;
    private TextMeshProUGUI characterName;


    public override void Initialize(IControllable[] _injectedElements)
    {
        base.Initialize(_injectedElements);
    }

}
