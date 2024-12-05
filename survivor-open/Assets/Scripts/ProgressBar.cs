using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image barImage;
    public void UpdateBar(float fillAmount)
    {
        if (barImage != null)
        {
            barImage.fillAmount = fillAmount;
        }
    }
}