using UnityEngine;
using DG.Tweening;

public class CharacterMotion : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 originalPos;
    private Vector3 originalScale;
    private Quaternion originalRot;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        // 记录立绘最原始的状态（位置、大小、旋转）
        if (rectTransform != null)
        {
            originalPos = rectTransform.anchoredPosition;
            originalScale = rectTransform.localScale;
            originalRot = rectTransform.localRotation;
        }
    }

    // === 核心：重置状态 ===
    // 每次播放新动画前，把立绘恢复原状，防止变形叠加（比如变大了没变回去）
    public void ResetState()
    {
        transform.DOKill(); // 杀掉所有动画
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, originalPos.y);
            rectTransform.localScale = originalScale;
            rectTransform.localRotation = originalRot;
        }
    }

    // 1. 原地跳跃 (开心/惊讶)
    public void PlayJump()
    {
        ResetState();
        rectTransform.DOJumpAnchorPos(rectTransform.anchoredPosition, 50f, 1, 0.5f);
    }

    // 2. 身体震动 (吓一跳/受伤)
    public void PlayShock()
    {
        ResetState();
        transform.DOPunchPosition(new Vector3(0, 30, 0), 0.5f, 20, 1);
    }

    // === 【新增】 3. 摇头 (否定/无奈) ===
    public void PlayNo()
    {
        ResetState();
        // Z轴旋转：像拨浪鼓一样左右摇
        // (0,0,10)表示旋转角度，0.5秒，震动5次
        transform.DOPunchRotation(new Vector3(0, 0, 10), 0.5f, 5, 1);
    }

    // === 【新增】 4. 点头 (肯定/行礼) ===
    public void PlayYes()
    {
        ResetState();
        // 稍微向下压一下位置，再旋转一点点，模拟鞠躬或点头
        Sequence seq = DOTween.Sequence();
        seq.Append(rectTransform.DOAnchorPosY(originalPos.y - 20, 0.1f)); // 下沉
        seq.Join(transform.DORotate(new Vector3(0, 0, -2), 0.1f));        // 稍微前倾
        seq.Append(rectTransform.DOAnchorPosY(originalPos.y, 0.2f));      // 回复
        seq.Join(transform.DORotate(new Vector3(0, 0, 0), 0.2f));
    }

    // === 【新增】 5. 凑近 (强调/激动/突脸) ===
    public void PlayLeanIn()
    {
        ResetState();
        // 放大 1.1 倍，产生“突然靠近镜头”的感觉
        // 使用 PunchScale 会自动放大后弹回原大小
        transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.5f, 2, 1);
    }

    // === 【新增】 6. 生气 (高频颤抖) ===
    public void PlayAngry()
    {
        ResetState();
        // 左右快速微小震动
        transform.DOPunchPosition(new Vector3(10, 0, 0), 1.0f, 50, 0);
    }
}