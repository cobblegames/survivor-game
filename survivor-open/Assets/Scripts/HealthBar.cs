using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    [SerializeField] Image barImage;

    public void UpdateBar(float fillAmount)
    {
        if (barImage != null)
        {
            barImage.fillAmount = fillAmount;
        }
    }
}
