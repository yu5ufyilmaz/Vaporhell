using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject ekipPanel; // Ekip paneli (Image)
    public GameObject kontrolPanel; // Kontrol paneli (Image)
    public float animationSpeed = 5f; // Animasyon hızı

    private Vector3 ekipStartPos; // Ekip panelinin başlangıç pozisyonu
    private Vector3 kontrolStartPos; // Kontrol panelinin başlangıç pozisyonu
    private Vector3 ekipTargetPos; // Ekip panelinin hedef pozisyonu
    private Vector3 kontrolTargetPos; // Kontrol panelinin hedef pozisyonu

    private bool ekipMoving = false; // Ekip paneli hareket ediyor mu?
    private bool kontrolMoving = false; // Kontrol paneli hareket ediyor mu?

    void Start()
    {
        // Başlangıç pozisyonlarını manuel olarak ayarla
        ekipStartPos = new Vector3(ekipPanel.transform.position.x, 1500, ekipPanel.transform.position.z);
        kontrolStartPos = new Vector3(kontrolPanel.transform.position.x, 1500, kontrolPanel.transform.position.z);

        ekipPanel.transform.position = ekipStartPos;
        kontrolPanel.transform.position = kontrolStartPos;

        // Hedef pozisyonlar
        ekipTargetPos = new Vector3(ekipStartPos.x, 540, ekipStartPos.z); // Y ekseninde hedef pozisyon
        kontrolTargetPos = new Vector3(kontrolStartPos.x, 540, kontrolStartPos.z); // Y ekseninde hedef pozisyon
    }

    void Update()
    {
        // Bu örnekte, tüm hareketler coroutine'ler tarafından yönetildiği için Update metodunu boş bırakıyoruz.
    }

    public void OnEkipButtonClicked()
    {
        // Eğer ekip paneli zaten açık veya hareket ediyorsa, kapat
        if (IsPanelOpen(ekipPanel, ekipTargetPos) || ekipMoving)
        {
            StartCoroutine(ClosePanel(ekipPanel, ekipStartPos, PanelType.Ekip));
            return;
        }

        // Eğer kontrol paneli açıksa, onu anında kapat
        if (IsPanelOpen(kontrolPanel, kontrolTargetPos) || kontrolMoving)
        {
            ClosePanelImmediately(kontrolPanel, kontrolStartPos, PanelType.Kontrol);
        }

        // Ekip panelini aç
        StartCoroutine(OpenPanel(ekipPanel, ekipTargetPos, PanelType.Ekip));
    }

    public void OnKontrolButtonClicked()
    {
        // Eğer kontrol paneli zaten açık veya hareket ediyorsa, kapat
        if (IsPanelOpen(kontrolPanel, kontrolTargetPos) || kontrolMoving)
        {
            StartCoroutine(ClosePanel(kontrolPanel, kontrolStartPos, PanelType.Kontrol));
            return;
        }

        // Eğer ekip paneli açıksa, onu anında kapat
        if (IsPanelOpen(ekipPanel, ekipTargetPos) || ekipMoving)
        {
            ClosePanelImmediately(ekipPanel, ekipStartPos, PanelType.Ekip);
        }

        // Kontrol panelini aç
        StartCoroutine(OpenPanel(kontrolPanel, kontrolTargetPos, PanelType.Kontrol));
    }

    // Coroutine to open a panel
    private System.Collections.IEnumerator OpenPanel(GameObject panel, Vector3 targetPos, PanelType panelType)
    {
        SetMovingFlag(panelType, true);

        while (Vector3.Distance(panel.transform.position, targetPos) > 0.01f)
        {
            panel.transform.position = Vector3.Lerp(panel.transform.position, targetPos, Time.deltaTime * animationSpeed);
            yield return null;
        }

        panel.transform.position = targetPos;
        SetMovingFlag(panelType, false);
    }

    // Coroutine to close a panel
    private System.Collections.IEnumerator ClosePanel(GameObject panel, Vector3 startPos, PanelType panelType)
    {
        SetMovingFlag(panelType, true);

        while (Vector3.Distance(panel.transform.position, startPos) > 0.01f)
        {
            panel.transform.position = Vector3.Lerp(panel.transform.position, startPos, Time.deltaTime * animationSpeed);
            yield return null;
        }

        panel.transform.position = startPos;
        SetMovingFlag(panelType, false);
    }

    // Method to close a panel immediately without animation
    private void ClosePanelImmediately(GameObject panel, Vector3 startPos, PanelType panelType)
    {
        // Stop any ongoing coroutine for closing/opening the panel
        StopAllCoroutines(); // Eğer daha spesifik coroutine'ler kullanmak isterseniz, her panel için ayrı coroutine'leri durdurabilirsiniz

        // Anında kapat
        panel.transform.position = startPos;
        SetMovingFlag(panelType, false);
    }

    // Helper method to set the moving flags based on panel type
    private void SetMovingFlag(PanelType panelType, bool isMoving)
    {
        switch (panelType)
        {
            case PanelType.Ekip:
                ekipMoving = isMoving;
                break;
            case PanelType.Kontrol:
                kontrolMoving = isMoving;
                break;
        }
    }

    // Helper methods to check panel states
    private bool IsPanelOpen(GameObject panel, Vector3 targetPos)
    {
        return Vector3.Distance(panel.transform.position, targetPos) < 0.01f;
    }

    private bool IsPanelClosed(GameObject panel, Vector3 startPos)
    {
        return Vector3.Distance(panel.transform.position, startPos) < 0.01f;
    }

    public void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("IntroScene"); // Belirttiğiniz sahne adını buraya yazın
    }

    public void OnExitButtonClicked()
    {
        Application.Quit(); // Oyundan çık
    }

    // Enum to differentiate between panels
    private enum PanelType
    {
        Ekip,
        Kontrol
    }
}
