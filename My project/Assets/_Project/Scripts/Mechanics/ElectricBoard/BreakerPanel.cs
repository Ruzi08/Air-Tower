using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BreakerPanel : MonoBehaviour
{
    [Header("Настройки")]
    public int minBroken = 3;
    public int maxBroken = 6;

    [Header("Компоненты")]
    public BreakerSwitch[] allSwitches;

    [Header("Камера")]
    public Transform panelLookPoint;
    public float cameraMoveSpeed = 5f;

    private bool isPanelOpen = false;
    private int brokenCount = 0;
    
    private Vector3 savedCameraPos;
    private Quaternion savedCameraRot;
    private Camera mainCamera;
    private Coroutine cameraMoveCoroutine;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();
        
        ResetAllSwitches();

        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerOut += HandlePowerOut;
        }
    }

    void Update()
    {
        if (isPanelOpen && Input.anyKeyDown && !Input.GetMouseButtonDown(0))
        {
            Debug.Log($"🚪 Выход из щитка по клавише: {Input.inputString}");
            ClosePanel();
        }
    }

    void OnDestroy()
    {
        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerOut -= HandlePowerOut;
        }
    }

    public void OnLidOpened()
    {
        Debug.Log("🔓 Крышка открыта, открываем панель");
        OpenPanel();
    }

    public void OnLidClosed()
    {
        Debug.Log("🔒 Крышка закрыта, закрываем панель");
        ClosePanel();
    }

    private void HandlePowerOut()
    {
        Debug.Log("🔧 Свет выключен! Ломаем рандомные рычажки...");
        RandomizeBrokenSwitches();
    }

    private void RandomizeBrokenSwitches()
    {
        ResetAllSwitches();

        int toBreak = Random.Range(minBroken, maxBroken + 1);
        List<BreakerSwitch> shuffled = allSwitches.ToList();

        for (int i = 0; i < shuffled.Count; i++)
        {
            var temp = shuffled[i];
            int rand = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[rand];
            shuffled[rand] = temp;
        }

        for (int i = 0; i < toBreak; i++)
        {
            shuffled[i].SetBroken();
        }

        UpdateBrokenCount();
        Debug.Log($"🔧 Щиток: {brokenCount} выбитых рычажков");
    }

    private void ResetAllSwitches()
    {
        foreach (var sw in allSwitches)
        {
            sw.ResetSwitch();
        }
        brokenCount = 0;
    }

    private void UpdateBrokenCount()
    {
        brokenCount = allSwitches.Count(s => s.isBroken && !s.isFixed);

        if (brokenCount == 0 && PowerManager.Instance != null && !PowerManager.Instance.HasPower())
        {
            FixAllPower();
        }
    }

    public void OnSwitchFixed()
    {
        UpdateBrokenCount();
    }

    private void FixAllPower()
    {
        Debug.Log("🔌 ВСЕ РЫЧАЖКИ ВКЛЮЧЕНЫ! Свет возвращается");
        PowerManager.Instance?.RestorePower();
    }

    private void OpenPanel()
    {
        if (isPanelOpen) return;
        
        isPanelOpen = true;
        
        if (mainCamera != null)
        {
            savedCameraPos = mainCamera.transform.position;
            savedCameraRot = mainCamera.transform.rotation;
            Debug.Log($"📷 Сохранил позицию камеры: {savedCameraPos}");
        }
        
        if (panelLookPoint != null && mainCamera != null)
        {
            if (cameraMoveCoroutine != null)
                StopCoroutine(cameraMoveCoroutine);
            cameraMoveCoroutine = StartCoroutine(MoveCameraToPoint());
        }
        
        // ✅ СКРЫВАЕМ ПРИЦЕЛ
        CrosshairController.Instance?.Hide();
        
        ShowCursor();
        LockPlayerControls(true);
        Debug.Log("🔓 Панель открыта");
    }

    private void ClosePanel()
    {
        if (!isPanelOpen) return;
        
        isPanelOpen = false;
        
        LidOpener lid = GetComponent<LidOpener>();
        if (lid != null && lid.IsOpen())
            lid.Interact();
        
        if (cameraMoveCoroutine != null)
            StopCoroutine(cameraMoveCoroutine);
        cameraMoveCoroutine = StartCoroutine(ReturnCameraToPlayer());
    }

    private IEnumerator MoveCameraToPoint()
    {
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        
        Vector3 targetPos = panelLookPoint.position;
        Quaternion targetRot = panelLookPoint.rotation;
        
        float progress = 0f;
        
        while (progress < 1f)
        {
            progress += Time.deltaTime * cameraMoveSpeed;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);
            
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, smoothProgress);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothProgress);
            
            yield return null;
        }
        
        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRot;
        Debug.Log("📷 Камера перемещена к щитку");
    }

    private IEnumerator ReturnCameraToPlayer()
    {
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        
        Vector3 targetPos = savedCameraPos;
        Quaternion targetRot = savedCameraRot;
        
        float progress = 0f;
        
        while (progress < 1f)
        {
            progress += Time.deltaTime * cameraMoveSpeed;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);
            
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, smoothProgress);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothProgress);
            
            yield return null;
        }
        
        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRot;
        
        // ✅ ПОКАЗЫВАЕМ ПРИЦЕЛ ОБРАТНО
        CrosshairController.Instance?.Show();
        
        HideCursor();
        LockPlayerControls(false);
        Debug.Log("📷 Камера вернулась к игроку");
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LockPlayerControls(bool locked)
    {
        FirstPersonController fps = FindObjectOfType<FirstPersonController>();
        if (fps != null)
        {
            if (locked)
                fps.LockAll();
            else
                fps.UnlockAll();
        }
    }

    public string GetDescription()
    {
        if (PowerManager.Instance != null && PowerManager.Instance.HasPower())
            return "⚡ Электричество работает";

        if (brokenCount > 0)
            return $"⚠️ Включить пробки ({brokenCount} выбито)";

        return "🔧 Осмотреть щиток";
    }
}