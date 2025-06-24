using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;
using System.Reflection;
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

    [Header("Input Harian")]
    public int cahayaCount;
    public int airCount;
    public int co2Count;

    public int maxCahaya = 3;
    public int maxAir = 2;
    public int maxCO2 = 3;

    [Header("Cooldown")]
    private bool isCahayaCooldown = false;
    private bool isCO2Cooldown = false;
    
   
    public enum Cuaca { Cerah, Mendung, Hujan, Polusi }
    public Cuaca cuacaHariIni;

    [Header("UI")]
    public TMP_Text waktuText;
    public TMP_Text cuacaText;
    public Slider energiSlider;
    public GameObject panelHasil;
    public TMP_Text hariText;
    public TMP_Text airText;
    public TMP_Text cahayaText;
    public TMP_Text co2Text;
    public TMP_Text oksigenText;
    public TMP_Text nilaiAkhirText;
    public TMP_Text skorTotalText;
    public TMP_Text fotosintesisText;
    public string cahayaString;
    public string co2String;

    [Header("Button")]
    public Button buttonCO2;
    public TMP_Text textCO2;
    public Button buttonCahaya;
    public TMP_Text textCahaya;
    public Button buttonAir;
    public TMP_Text textAir;

    public TMP_Text skipDay;

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
            currentHour = 6;
            EndOfDay();
            currentDay++;
            if (currentDay > 10)
            {
                EndGame();
                return;
            }
            ResetHarian();
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
    }

    public void SerapCahaya()
    {
        cahayaCount++;
        cahayaTotal++;
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
            imageSoil.sprite = spriteSoil[1];
            StartCoroutine(KembalikanTanahKering());
        }
        TambahEnergi(4);
    }

    IEnumerator KembalikanTanahKering()
    {
        yield return new WaitForSeconds(1f);
        imageSoil.sprite = spriteSoil[0];
    }

    public void SerapCO2()
    {
        co2Count++;
        co2Total++;
        StartCoroutine(CO2Cooldown());
        TambahEnergi(3);
    }

    public void CuacaChanger()
    {
        if (cuacaHariIni == Cuaca.Hujan)
        {
            buttonCahaya.interactable = false;
            textCahaya.text = "Sedang Hujan";

        } else if (cahayaCount >= maxCahaya)
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

        if (cuacaHariIni == Cuaca.Hujan && airCount <= maxAir) 
        {
            buttonAir.interactable = false;
            airCount += maxAir;
            airTotal += maxAir;
            TambahEnergi(8);
            textAir.text = "Air Terpenuhi";
        }
         else if (airCount >= maxAir)
        {
            buttonAir.interactable = false;
            textAir.text = "Air Terpenuhi";
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

        if (fotosintesisEnergy <= 60) index = 0;
        else if (fotosintesisEnergy <= 120) index = 1;
        else if (fotosintesisEnergy <= 180) index = 2;
        else if (fotosintesisEnergy <= 240) index = 3;
        else if (fotosintesisEnergy <= 360) index = 4;

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
        if (currentDay >= 10)
        {
            //StopAllCoroutines();
            //currentHour = 6;
            //ResetHarian();
            //currentDay = 1;
            //fotosintesisEnergy = 0;
            //levelTanaman = 0;
            //Time.timeScale = 1;
            //skipDay.text = "Skip the day";
            //panelHasil.SetActive(false);
            levelLoader.LoadLevel(1);
        }
        else if (currentDay == 9)
        {
            skipDay.text = "Restart game";
            currentDay++;
            EndGame();
            return;
        }
        else
        {
            StopAllCoroutines();
            currentHour = 6;
            EndOfDay();
            currentDay++;
            ResetHarian();
        }
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
        oksigenTotal += UnityEngine.Random.Range(1, 3); // Simulasi produksi O₂
        fotosintesisEnergy = Mathf.Clamp(fotosintesisEnergy, 0, 360);
    }

    void EndOfDay()
    {
        if (fotosintesisEnergy >= currentDay * 10)
            levelTanaman = Mathf.Min(levelTanaman + 1, 10);

    }

    void ResetHarian()
    {
        cahayaCount = 0;
        airCount = 0;
        co2Count = 0;
        timer = 0f;
        isCO2Cooldown = false;
        isCahayaCooldown = false;
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

    void EndGame()
    {
        Time.timeScale = 0;
        TotalCahaya();
        TotalCO2();

        string nilai = "";
        int totalScore = fotosintesisEnergy;

        if (totalScore >= 360) nilai = "A";
        else if (totalScore >= 240) nilai = "B";
        else if (totalScore >= 180) nilai = "C";
        else if (totalScore >= 120) nilai = "D";
        else nilai = "E";

        panelHasil.SetActive(true);
        //hasilText.text = $"Tanaman mencapai level {levelTanaman}/5\n" +
        //                 $"Energi: {fotosintesisEnergy}\n" +
        //                 $"Oksigen: {oksigenTotal}\n" +
        //                 $"Hari Efektif: {currentDay - 1}\n" +
        //                 $"Nilai Akhir: {nilai}\n" +
        //                 $"Total Skor: {totalScore} \n";

        hariText.text = "Hari bertahan hidup : " + currentDay + "/10";
        airText.text = "Siraman Air : " + airTotal + "x";
        cahayaText.text = "Serapan Cahaya : " + $"{cahayaString}" + " (" + Math.Round(((float)cahayaTotal / 30) * 100f) + "%" + ")" ;
        co2Text.text = "Pengambilan C02 : " + $"{co2String}" + " (" + Math.Round(((float)co2Total / 30) * 100f) + "%" + ")" ;
        oksigenText.text = "Jumlah oksigen dihasilkan : " + Math.Round(((float)oksigenTotal / 100) * 100f) + "%";
        fotosintesisText.text = "Tingkat efisiensi : " + Math.Round(((float)fotosintesisEnergy / 360) * 100f) + "%";
        nilaiAkhirText.text = "Nilai Akhir : " + nilai;
        skorTotalText.text = "Total Skor : " + totalScore;

    }
}