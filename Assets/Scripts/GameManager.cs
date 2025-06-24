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
    public TMP_Text hasilText;

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
        energiSlider.value = fotosintesisEnergy / 100f;
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
        if (cuacaHariIni == Cuaca.Hujan || cahayaCount >= maxCahaya || isCahayaCooldown)
        {
            buttonCahaya.interactable = false;
            textCahaya.text = "Cooldown";

        }
        else
        {
            buttonCahaya.interactable = true;
            textCahaya.text = "Serap Cahaya";
        }

        if (cuacaHariIni == Cuaca.Polusi || co2Count >= maxCO2 || isCO2Cooldown)
        {
            buttonCO2.interactable = false;
            textCO2.text = "Cooldown";
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

    public void PlantChanger()
    {
        int index = 0;

        if (levelTanaman <= 2) index = 0;
        else if (levelTanaman <= 4) index = 1;
        else if (levelTanaman <= 6) index = 2;
        else if (levelTanaman <= 8) index = 3;
        else if (levelTanaman <= 10) index = 4;

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
        fotosintesisEnergy = Mathf.Clamp(fotosintesisEnergy, 0, 100);
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

        string nilai = "C";
        int totalScore = fotosintesisEnergy;

        if (totalScore >= 90) nilai = "A";
        else if (totalScore >= 70) nilai = "B";
        else if (totalScore >= 50) nilai = "C";
        else if (totalScore >= 30) nilai = "D";
        else nilai = "E";

        panelHasil.SetActive(true);
        hasilText.text = $"Tanaman mencapai level {levelTanaman}/5\n" +
                         $"Energi: {fotosintesisEnergy}\n" +
                         $"Oksigen: {oksigenTotal}\n" +
                         $"Hari Efektif: {currentDay - 1}\n" +
                         $"Nilai Akhir: {nilai}\n" +
                         $"Total Skor: {totalScore} \n";
    }
}