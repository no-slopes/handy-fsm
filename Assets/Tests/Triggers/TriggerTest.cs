using HandyFSM;
using UnityEngine;
using UnityEngine.UI;

public class TriggerTest : MonoBehaviour
{
    #region Inspector

    [SerializeField]
    private Button _button;

    [SerializeField]
    private HandyFSMBrain _brain;

    #endregion

    #region Behaviour

    private void OnEnable()
    {
        _button.onClick.AddListener(OnButtonClick);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnButtonClick);
    }

    #endregion

    public void SetBrain(HandyFSMBrain brain)
    {
        _brain = brain;
    }

    private void OnButtonClick()
    {
        if (_brain == null) return;
        _brain.Triggers.Squeeze("tap", new FloatTriggerData(Random.Range(0, 100)));
    }

}