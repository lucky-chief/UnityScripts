using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class Button3D : MonoBehaviour
{
    [Header("视觉配置")]
    public Color normalColor = Color.white;
    public Color pressedColor = Color.gray;
    public float pressDepth = 0.1f; // 按下时的深度偏移
    public float animationSpeed = 10f; // 动画速度
    
    [Header("交互配置")]
    public float longPressTime = 1f; // 长按触发时间
    
    [Header("事件")]
    public UnityEvent OnClick;
    public UnityEvent OnLongPress;
    public UnityEvent OnPressStart; // 按下开始
    public UnityEvent OnPressEnd;   // 按下结束
    
    // 私有变量
    private Renderer buttonRenderer;
    private Vector3 originalPosition;
    private Vector3 pressedPosition;
    private bool isPressed = false;
    private bool isLongPressed = false;
    private float pressStartTime;
    private Coroutine longPressCoroutine;
    
    // 材质属性
    private Material buttonMaterial;
    private Color currentColor;
    
    void Start()
    {
        Initialize();
    }
    
    void Initialize()
    {
        // 获取组件
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer == null)
        {
            Debug.LogError("Button3D: 找不到Renderer组件！");
            return;
        }
        
        // 创建材质实例（避免影响其他对象）
        buttonMaterial = buttonRenderer.material;
        buttonRenderer.material = buttonMaterial;
        
        // 记录原始位置
        originalPosition = transform.localPosition;
        pressedPosition = originalPosition + Camera.main.transform.forward * pressDepth;
        
        // 设置初始颜色
        currentColor = normalColor;
        SetButtonColor(currentColor);
        
        // 确保有Collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
    }
    
    void Update()
    {
        HandleInput();
        UpdateVisuals();
    }
    
    void HandleInput()
    {
        // 鼠标输入
        if (Input.GetMouseButtonDown(0))
        {
            CheckMouseClick();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isPressed)
            {
                OnButtonRelease();
            }
        }
        
        // 触摸输入
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                CheckTouchClick(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (isPressed)
                {
                    OnButtonRelease();
                }
            }
        }
    }
    
    void CheckMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                OnButtonPress();
            }
        }
    }
    
    void CheckTouchClick(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                OnButtonPress();
            }
        }
    }
    
    void OnButtonPress()
    {
        if (isPressed) return;
        
        isPressed = true;
        isLongPressed = false;
        pressStartTime = Time.time;
        
        // 启动长按检测
        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
        }
        longPressCoroutine = StartCoroutine(LongPressDetection());
        
        // 触发按下事件
        OnPressStart?.Invoke();
    }
    
    void OnButtonRelease()
    {
        if (!isPressed) return;
        
        // 停止长按检测
        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
            longPressCoroutine = null;
        }
        
        // 如果没有触发长按，则触发点击事件
        if (!isLongPressed)
        {
            OnClick?.Invoke();
        }
        
        isPressed = false;
        isLongPressed = false;
        
        // 触发释放事件
        OnPressEnd?.Invoke();
    }
    
    IEnumerator LongPressDetection()
    {
        yield return new WaitForSeconds(longPressTime);
        
        if (isPressed && !isLongPressed)
        {
            isLongPressed = true;
            OnLongPress?.Invoke();
        }
    }
    
    void UpdateVisuals()
    {
        // 更新位置
        Vector3 targetPosition = isPressed ? pressedPosition : originalPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, 
            Time.deltaTime * animationSpeed);
        
        // 更新颜色
        Color targetColor = isPressed ? pressedColor : normalColor;
        if (currentColor != targetColor)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * animationSpeed);
            SetButtonColor(currentColor);
        }
    }
    
    void SetButtonColor(Color color)
    {
        if (buttonMaterial != null)
        {
            buttonMaterial.color = color;
        }
    }
    
    // 公共方法 - 程序化触发
    public void SimulateClick()
    {
        OnClick?.Invoke();
    }
    
    public void SimulateLongPress()
    {
        OnLongPress?.Invoke();
    }
    
    // 设置颜色的公共方法
    public void SetNormalColor(Color color)
    {
        normalColor = color;
        if (!isPressed)
        {
            SetButtonColor(color);
        }
    }
    
    public void SetPressedColor(Color color)
    {
        pressedColor = color;
        if (isPressed)
        {
            SetButtonColor(color);
        }
    }
    
    void OnDestroy()
    {
        // 清理协程
        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
        }
    }
}
