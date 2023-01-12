using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponAmmoUI : MonoBehaviour {

    [SerializeField] GameObject[] sliderObjects;
    [SerializeField] GameObject[] textObjects;


    public void UpdateUI(Weapon weapon, int index) {
        textObjects[index].GetComponent<Text>().text = weapon.ammoCount.ToString();
        sliderObjects[index].GetComponent<Slider>().value = (float)weapon.currentClipAmmo/(float)weapon.clipSize;
    }

    public void UpdateAllUI(Weapon[] weapons) {
        for (int i = 0; i < weapons.Length; i++) {
            textObjects[i].GetComponent<Text>().text = weapons[i].ammoCount.ToString();
            sliderObjects[i].GetComponent<Slider>().value = (float)weapons[i].currentClipAmmo / (float)weapons[i].clipSize;
        }
    }
}
