using UniRx;
using UniRx.Triggers;
using UniSoftwareKeyboardArea;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileKeyboardSample : MonoBehaviour
{
    [SerializeField] private InputField chatInput = default!;
    [SerializeField] private Button submitButton = default!;
    [SerializeField] private Text chatPrefab = default!;
    [SerializeField] private Transform chatContent = default!;
    [SerializeField] private RectTransform chatContainer = default!;
#if !UNITY_EDITOR
    private TouchScreenKeyboard _keyboard = default!;
#endif

    void Start()
    {
#if UNITY_IOS && !UNITY_EDITOR
        submitButton.OnPointerEnterAsObservable()
            .Subscribe(_ => { _keyboard = TouchScreenKeyboard.Open(string.Empty, TouchScreenKeyboardType.Search); })
            .AddTo(this);
        submitButton.OnPointerUpAsObservable()
            .Subscribe(_ =>
            {
                // ボタンを押さずに離した場合にフォーカスを戻す
                EventSystem.current.SetSelectedGameObject(chatInput.gameObject);
            }).AddTo(this);
#endif

        submitButton.onClick.AsObservable()
            .Subscribe(async unit => Send())
            .AddTo(this);

#if !UNITY_EDITOR
        // keyboardの参照を取得
        chatInput.OnSelectAsObservable()
            .Subscribe(_ => { _keyboard = TouchScreenKeyboard.Open(string.Empty, TouchScreenKeyboardType.Search); })
            .AddTo(this);

        // keyboardステータスを監視
        Observable.EveryUpdate()
            .Where(_ => _keyboard != null)
            .Select(_ => _keyboard.status)
            .DistinctUntilChanged()
            .Subscribe(status =>
            {
                switch (status)
                {
                    case TouchScreenKeyboard.Status.Visible:
#if UNITY_ANDROID
                        // Androidではキーボード表示中にUI要素をタッチできない
                        submitButton.interactable = false;
#endif
                        break;
                    case TouchScreenKeyboard.Status.Done:
                        Send();
                        submitButton.interactable = true;
                        break;
                    case TouchScreenKeyboard.Status.Canceled:
                    case TouchScreenKeyboard.Status.LostFocus:
                        submitButton.interactable = true;
                        break;
                }
            })
            .AddTo(this);
#endif

        Observable.EveryUpdate()
            .Select(_ => SoftwareKeyboardArea.GetHeight())
            .DistinctUntilChanged()
            .Subscribe(height =>
            {
                var safeAreaBottom = Screen.safeArea.y;
                var resolutionHeight = chatContainer
                    .GetComponentInParent<CanvasScaler>()
                    .GetComponent<RectTransform>().sizeDelta.y;
                var rate = resolutionHeight / Screen.height;

                // キーボードが非表示の時は 0
                var margin = (int) (height <= 0 ? 0 : (height - safeAreaBottom) * rate);
                chatContainer.sizeDelta = new Vector2(0, -margin);

                Debug.Log($"height: {height}");
                Debug.Log($"safeAreaBottom: {safeAreaBottom}");
                Debug.Log($"resolutionHeight: {resolutionHeight}");
                Debug.Log($"Screen.height: {Screen.height}");
                Debug.Log($"rate: {rate}");
                Debug.Log($"(height + safeAreaBottom) * rate: {(height - safeAreaBottom) * rate}");
            }).AddTo(this);
    }

    private void Send()
    {
#if !UNITY_EDITOR
        _keyboard = TouchScreenKeyboard.Open(string.Empty, TouchScreenKeyboardType.Search);
#endif
        EventSystem.current.SetSelectedGameObject(chatInput.gameObject);

        // 中身があれば送信
        if (!string.IsNullOrWhiteSpace(chatInput.text))
        {
            // サンプルなので実際にはローカル上でテキスト生成のみ
            var chat = Instantiate(chatPrefab, chatContent);
            chat.text = chatInput.text;
        }

        chatInput.text = string.Empty;
    }
}