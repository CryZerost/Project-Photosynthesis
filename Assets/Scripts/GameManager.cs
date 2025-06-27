using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;


public class GameManager : MonoBehaviour
{

    [Header("Scripts")]
    public LevelLoader levelLoader;

    [Header("ImageIngame")]
    [SerializeField] Image imageBackground;
    [SerializeField] Image imageSoil;
 
    public Sprite[] spriteBackground;
    public GameObject[] objectPlant;
    public GameObject[] objectRoot;
    public Sprite[] spriteSoil;
    public GameObject fadeDay;
    public TMP_Text fadeText;
    

    [Header("Waktu")]
    public int currentDay = 1;
    public int currentHour = 6;
    public float realTimePerHour = 60f; // 1 jam = 1 menit nyata
    private float timer;

    [Header("Pertumbuhan & Energi")]
    public int fotosintesisEnergy = 0;
    public int oksigenTotal = 0;
    public int co2Total = 0;
    public int cahayaTotal = 0;
    public int airTotal = 0;
    public int levelTanaman = 1;
    public int randomTotal = 0;

    [Header("Input Harian")]
    public int cahayaCount;
    public int airCount;
    public int co2Count;

    public int maxCahaya = 5;
    public int maxAir = 4;
    public int maxCO2 = 5;

    [Header("Cooldown")]
    private bool isCahayaCooldown = false;
    private bool isCO2Cooldown = false;
    private bool isAirCooldown = false;
   
   
    public enum Cuaca { Cerah, Mendung, Hujan, Polusi }
    public Cuaca cuacaHariIni;

    [Header("UI")]
    public TMP_Text waktuText;
    public TMP_Text cuacaText;
    public GameObject panelHasil;
    public TMP_Text hariText;
    public TMP_Text cahayaText;
    public TMP_Text airText;
    public TMP_Text co2Text;
    public TMP_Text oksigenText;
    public TMP_Text nilaiAkhirText;
    public TMP_Text skorTotalText;
    public TMP_Text fotosintesisText;
    public string cahayaString;
    public string co2String;

    [Header("UI Slider")]
    public Slider energiSlider;
    public Slider cahayaSlider;
    public Slider airSlider;
    public Slider co2Slider;

    [Header("Button")]
    public Button buttonCO2;
    public TMP_Text textCO2;
    public Button buttonCahaya;
    public TMP_Text textCahaya;
    public Button buttonAir;
    public TMP_Text textAir;
    public TMP_Text skipDay;
    public GameObject buttonSkip;


    [Header("Effect 3 Utama")]
    public Transform[] effectPosition;
    public Transform targetPosition;
    public GameObject[] effectObjects;
    public GameObject effectMatahari;
    public GameObject effectMatahariPos;
    public GameObject effectAwan;
    public GameObject effectAirPos;
    public GameObject effectAir;
    public GameObject effectAirPosTarget;
    public GameObject effectSiram;
    public GameObject effectSiramPos;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] swooshClip;
    public AudioClip co2Clip;
    public AudioClip airClip;

    [Header("HasilGambar")]
    public GameObject tanamanSubur;
    public GameObject tanamanKering;
    public GameObject tanamanLayu;

    [Header("Batas Ideal Masukan Harian")]
    public int idealCahaya = 5;
    public int idealAir = 5;
    public int idealCO2 = 5;
    public int toleransi = 1;


    void Start()
    {
        SetCuacaBaru();
        UpdateUI();
        PlantChanger();
        BackgroundChanger();

    }

    void Update()
    {
        timer += Time.deltaTime;
        UpdateUI();
        if (timer >= realTimePerHour)
        {
            timer = 0f;
            AdvanceHour();
        }

        CuacaChanger();
    }

    void AdvanceHour()
    {
        currentHour++;
        if (currentHour >= 17)
        {
            SkipDay();
        }

        UpdateUI();
    }

    void SetCuacaBaru()
    {
        int roll = UnityEngine.Random.Range(0, 100);
        if (roll < 40) cuacaHariIni = Cuaca.Cerah;
        else if (roll < 65) cuacaHariIni = Cuaca.Mendung;
        else if (roll < 85) cuacaHariIni = Cuaca.Hujan;
        else cuacaHariIni = Cuaca.Polusi;

    }

    void UpdateUI()
    {
        waktuText.text = $"Hari {currentDay} - Jam {(currentHour < 10 ? "0" : "" )}{currentHour}:{(Mathf.RoundToInt(timer) < 10 ? "0" : "")}{Mathf.RoundToInt(timer)}";
        cuacaText.text = $"Cuaca: {cuacaHariIni}";
        energiSlider.value = fotosintesisEnergy / 360f;

        
        const float sliderMax = 10f;

        cahayaSlider.maxValue = sliderMax;
        airSlider.maxValue = sliderMax;
        co2Slider.maxValue = sliderMax;

        
        cahayaSlider.value = cahayaTotal;
        airSlider.value = airTotal;
        co2Slider.value = co2Total;


        if (cuacaHariIni == Cuaca.Hujan)
        {
            effectMatahariPos.SetActive(false);
        }
        else
        {
            effectMatahariPos.SetActive(true);
        }

        if (cuacaHariIni == Cuaca.Mendung)
        {
            effectAwan.SetActive(true);
        }
        else
        {
            effectAwan.SetActive(false);
        }


    }

    public void SerapCahaya()
    {
        cahayaCount++;
        cahayaTotal++;
        EffectMathari();
        StartCoroutine(CahayaCooldown());
        TambahEnergi(5);
    }

    public void SiramAir()
    {

        airCount++;
        airTotal++;
        if (airCount >= maxAir)
        {
            imageSoil.sprite = spriteSoil[1];
        }
        else
        {
            StartCoroutine(KembalikanTanahKering());
        }
        EffectAir();
        TambahEnergi(4);
    }

    public void SerapCO2()
    {
        co2Count++;
        co2Total++;
        EffectSerapCO2();
        StartCoroutine(CO2Cooldown());
        TambahEnergi(3);
    }


    IEnumerator KembalikanTanahKering()
    {
        isAirCooldown = true;
        imageSoil.sprite = spriteSoil[1];
        yield return new WaitForSeconds(2f);
        isAirCooldown = false;
        imageSoil.sprite = spriteSoil[0];

    }


    public void CuacaChanger()
    {
        if (cuacaHariIni == Cuaca.Hujan)
        {
            buttonCahaya.interactable = false;
            textCahaya.text = "Sedang Hujan";

        }
        else if (cahayaCount >= maxCahaya)
        {
            buttonCahaya.interactable = false;
            textCahaya.text = "Cahaya Terpenuhi";
        }
        else if (isCahayaCooldown)
        {
            buttonCahaya.interactable = false;
            textCahaya.text = "Cooldown 2d";
        }
        else
        {
            buttonCahaya.interactable = true;
            textCahaya.text = "Serap Cahaya";
        }

        if (cuacaHariIni == Cuaca.Polusi)
        {
            buttonCO2.interactable = false;
            textCO2.text = "Polusi Udara";
        }
        else if (co2Count >= maxCO2)
        {
            buttonCO2.interactable = false;
            textCO2.text = "CO2 Terpenuhi";
        }
        else if (isCO2Cooldown)
        {
            buttonCO2.interactable = false;
            textCO2.text = "Cooldown 3d";
        }
        else
        {
            buttonCO2.interactable = true;
            textCO2.text = "Serap CO2";
        }

        if (cuacaHariIni == Cuaca.Hujan && airCount <= idealAir)
        {
            buttonAir.interactable = false;
            airCount += maxAir;
            airTotal = idealAir;
            TambahEnergi(8);
            textAir.text = "Air Terpenuhi";
        }
        else if (airCount >= maxAir)
        {
            buttonAir.interactable = false;
            textAir.text = "Air Terpenuhi";
        }
        else if (isAirCooldown)
        {
            buttonAir.interactable = false;
            textAir.text = "Cooldown 2d";
        }
        else
        {
            buttonAir.interactable = true;
            textAir.text = "Siram Air";
        }
    }

    public void TotalCahaya()
    {
        int persen = Mathf.RoundToInt(((float)cahayaTotal / 30) * 100f);

        switch (persen)
        {
            case int n when (n < 10):
                cahayaString = "Sangat Buruk";
                Debug.Log("⚠️ Cahaya <10%");
                break;

            case int n when (n < 25):
                cahayaString = "Buruk";
                Debug.Log("⚠️ Cahaya <25%");
                break;

            case int n when (n < 50):
                cahayaString = "Kurang Baik";
                Debug.Log("⚠️ Cahaya <50%");
                break;

            case int n when (n < 75):
                cahayaString = "Cukup";
                Debug.Log("⚠️ Cahaya <75%");
                break;

            case int n when (n < 90):
                cahayaString = "Baik";
                Debug.Log("✅ Cahaya Baik");
                break;

            case int n when (n <= 100):
                cahayaString = "Optimal";
                Debug.Log("🌞 Cahaya Optimal");
                break;

            default:
                cahayaString = "Data tidak valid";
                Debug.LogWarning("Nilai di luar jangkauan");
                break;
        }
    }

    public void TotalCO2()
    {
        int persen = Mathf.RoundToInt(((float)co2Total / 30) * 100f);

        switch (persen)
        {
            case int n when (n < 10):
                co2String = "Sangat Buruk";
                Debug.Log("⚠️ Cahaya <10%");
                break;

            case int n when (n < 25):
                co2String = "Buruk";
                Debug.Log("⚠️ Cahaya <25%");
                break;

            case int n when (n < 50):
                co2String = "Kurang Baik";
                Debug.Log("⚠️ Cahaya <50%");
                break;

            case int n when (n < 75):
                co2String = "Cukup";
                Debug.Log("⚠️ Cahaya <75%");
                break;

            case int n when (n < 90):
                co2String = "Baik";
                Debug.Log("✅ Cahaya Baik");
                break;

            case int n when (n <= 100):
                co2String = "Optimal";
                Debug.Log("🌞 Cahaya Optimal");
                break;

            default:
                co2String = "Data tidak valid";
                Debug.LogWarning("Nilai di luar jangkauan");
                break;
        }
    }

    public void PlantChanger()
    {
        int index = 0;

        if (currentDay <= 2) index = 0;
        else if (currentDay <= 4) index = 1;
        else if (currentDay <= 6) index = 2;
        else if (currentDay <= 8) index = 3;
        else if (currentDay <= 10) index = 4;

        for (int i = 0; i < objectPlant.Length; i++)
        {
            objectPlant[i].SetActive(false);
        }

        for (int i = 0; i < objectRoot.Length; i++)
        {
            objectRoot[i].SetActive(false);
        }

        objectRoot[index].SetActive(true);
        objectPlant[index].SetActive(true);
    }

    public void BackgroundChanger()
    {
        switch (cuacaHariIni) 
        {
            case Cuaca.Hujan:
                imageBackground.sprite = spriteBackground[3];
                imageSoil.sprite = spriteSoil[1];
                break;
            case Cuaca.Polusi:
                imageBackground.sprite = spriteBackground[2];
                imageSoil.sprite = spriteSoil[0];
                break;
            case Cuaca.Mendung:
                imageBackground.sprite = spriteBackground[1];
                imageSoil.sprite = spriteSoil[0];
                break;
            case Cuaca.Cerah:
                imageBackground.sprite = spriteBackground[0];
                imageSoil.sprite = spriteSoil[0];
                break;
        }

    }
    public void SkipDay()
    {
        if (currentDay > 1)
        {
            if (EvaluasiStatus(cahayaTotal, idealCahaya) == "Cukup" && EvaluasiStatus(airTotal, idealAir) == "Cukup"
                || EvaluasiStatus(cahayaTotal, idealCahaya) == "Cukup" && EvaluasiStatus(co2Total, idealCO2) == "Cukup"
                || EvaluasiStatus(co2Total, idealCO2) == "Cukup" && EvaluasiStatus(airTotal, idealAir) == "Cukup")
            {
                if (currentDay == 10)
                {
                    skipDay.text = "No more day!";
                    buttonSkip.SetActive(false);
                    EndGame();
                    return;
                }
                else
                {
                    StopAllCoroutines();
                    currentHour = 6;
                    EndOfDay();
                    currentDay++;
                    FadeDay();
                    ResetHarian();
                }
            }
            else
            {
                skipDay.text = "No more day!";
                buttonSkip.SetActive(false);
                EndGame();
                return;
            }
        }
        else
        {
            StopAllCoroutines();
            currentHour = 6;
            EndOfDay();
            currentDay++;
            FadeDay();
            ResetHarian();
        }


    }

    public void FadeDay()
    {
        StartCoroutine(FadeCooldown());
        fadeText.text = "DAY " + currentDay;
    }

    IEnumerator FadeCooldown()
    {
        fadeDay.SetActive(true);
        yield return new WaitForSeconds(2f);
        fadeDay.SetActive(false);
    }

    public void ResetGame()
    {
        StopAllCoroutines();
        currentHour = 6;
        ResetHarian();
        currentDay = 1;
        fotosintesisEnergy = 0;
        levelTanaman = 0;
        Time.timeScale = 1;
        skipDay.text = "Skip the day";
        panelHasil.SetActive(false);
    }

    IEnumerator CahayaCooldown()
    {
        isCahayaCooldown = true;
        yield return new WaitForSeconds(2f);
        isCahayaCooldown = false;

    }

    IEnumerator CO2Cooldown()
    {
        isCO2Cooldown = true;
        yield return new WaitForSeconds(3f);
        isCO2Cooldown = false;
    }

    void TambahEnergi(int nilai)
    {
        fotosintesisEnergy += nilai;
        oksigenTotal += UnityEngine.Random.Range(1, 3); 
        fotosintesisEnergy = Mathf.Clamp(fotosintesisEnergy, 0, 300);
    }

    void EndOfDay()
    {
        if (fotosintesisEnergy >= currentDay * 10)
            levelTanaman = Mathf.Min(levelTanaman + 1, 10);
    }

    string EvaluasiStatus(int nilai, int ideal)
    {
        if (nilai < ideal - toleransi)
            return "Kurang";
        else if (nilai > ideal + toleransi)
            return "Terlalu Banyak";
        else
            return "Cukup";
    }

    string EvaluasiStatusKeseluruhan(string statusCahaya, string statusAir, string statusCO2)
    {
        if (statusCahaya == "Cukup" && statusAir == "Cukup" && statusCO2 == "Cukup")
            return "Subur";
        else if (statusCahaya == "Kurang" || statusAir == "Kurang" || statusCO2 == "Kurang")
            return "Kering";
        else
            return "Layu";
    }


    void ResetHarian()
    {
        cahayaCount = 0;
        airCount = 0;
        co2Count = 0;
        timer = 0f;
        isCO2Cooldown = false;
        isCahayaCooldown = false;
        isAirCooldown = false;

        // Kurangi total harian agar ada decay
        cahayaTotal = Mathf.Max(0, cahayaTotal - 2);
        airTotal = Mathf.Max(0, airTotal - 1);
        co2Total = Mathf.Max(0, co2Total - 2);

        PlantChanger();
        SetCuacaBaru();
        BackgroundChanger();
        ResetButton();
        UpdateUI();
    }


    void ResetButton()
    {
        buttonCahaya.interactable = true;
        buttonCO2.interactable = true;
        buttonAir.interactable = true;  
    }

    public void EffectSerapCO2()
    {
        ShowSerapCO2();
        audioSource.PlayOneShot(swooshClip[UnityEngine.Random.Range(0, swooshClip.Length - 1)]);
        for (int i = 0; i < effectObjects.Length; i++)
        {
            effectObjects[i].transform.position = effectPosition[i].transform.position;
        }

        for (int i = 0; i < effectObjects.Length; i++)
        {
            LeanTween.move(effectObjects[i], targetPosition, 1f).setOnComplete(() => audioSource.PlayOneShot(co2Clip));
        }

        LeanTween.delayedCall(1f, () => HideSerapCO2());
    }

    public void EffectAir()
    {
        effectAir.transform.position = effectAirPosTarget.transform.position;
        LeanTween.rotateZ(effectAir, -45f, 0.5f)
            .setOnComplete(() => EffectSiram());

    }

    public void EffectSiram()
    {
        effectSiram.SetActive(true);
        audioSource.PlayOneShot(airClip);
        effectSiram.transform.position = effectSiramPos.transform.position;
        LeanTween.move(effectSiram, targetPosition, 1f).setOnComplete(() => audioSource.PlayOneShot(co2Clip));
        LeanTween.delayedCall(1f, () => effectSiram.SetActive(false));
        LeanTween.rotateZ(effectAir, 0f, 0.5f).setDelay(1.5f)
        .setOnComplete(() => effectAir.transform.position = effectAirPos.transform.position);
    }

    public void EffectMathari()
    {
        effectMatahari.SetActive(true);
        effectMatahari.transform.position = effectMatahariPos.transform.position;
        audioSource.PlayOneShot(swooshClip[UnityEngine.Random.Range(0, swooshClip.Length - 1)]);
        LeanTween.move(effectMatahari, targetPosition, 1f).setOnComplete(() => audioSource.PlayOneShot(co2Clip));
        LeanTween.delayedCall(1f, () => effectMatahari.SetActive(false));
    }

    public void HideSerapCO2()
    {
        for (int i = 0; i < effectObjects.Length; i++)
        {
           effectObjects[i].gameObject.SetActive(false);
        }
    }

    public void ShowSerapCO2()
    {
        for (int i = 0; i < effectObjects.Length; i++)
        {
            effectObjects[i].gameObject.SetActive(true);
        }
    }

    void EndGame()
    {
        Time.timeScale = 0;
        TotalCahaya();
        TotalCO2();
        string statusCahaya = EvaluasiStatus(cahayaTotal, idealCahaya);
        string statusAir = EvaluasiStatus(airTotal, idealAir);
        string statusCO2 = EvaluasiStatus(co2Total, idealCO2);

        string statusKeseluruhan = EvaluasiStatusKeseluruhan(statusCahaya, statusAir, statusCO2);

        if (statusKeseluruhan == "Subur")
        {
            tanamanSubur.SetActive(true) ;
        }
        else if (statusKeseluruhan == "Layu")
        {
            tanamanLayu.SetActive(true) ;
        }
        else
        {
            tanamanKering.SetActive(true) ;
        }

        string nilai = "";
        int totalScore = fotosintesisEnergy;

        if (totalScore >= 240) nilai = "A";
        else if (totalScore >= 180) nilai = "B";
        else if (totalScore >= 120) nilai = "C";
        else if (totalScore >= 60) nilai = "D";
        else nilai = "E";

        if (statusKeseluruhan == "Subur") totalScore += 30;
        else if (statusKeseluruhan == "Layu") totalScore += 10;

        
        panelHasil.SetActive(true);
        hariText.text = "Hari bertahan hidup : " + currentDay + "/10";
        cahayaText.text = "Serapan Cahaya : " + statusCahaya + " (" + Math.Round(((float)cahayaTotal / 6) * 100f) + "%)";
        co2Text.text = "Pengambilan CO2 : " + statusCO2 + " (" + Math.Round(((float)co2Total / 6) * 100f) + "%)";
        airText.text = "Siraman Air : " + statusAir + " (" + airTotal + "x)";
        oksigenText.text = "Jumlah oksigen dihasilkan : " + Math.Round(((float)oksigenTotal / 100) * 100f) + "%";
        fotosintesisText.text = "Keseimbangan Tanaman : " + statusKeseluruhan;
        nilaiAkhirText.text = "Nilai Akhir : " + nilai;
        skorTotalText.text = "Total Skor : " + totalScore;

    }
}