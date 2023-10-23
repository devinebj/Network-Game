using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpFireRate : BasePowerUp {
    [SerializeField] float timeBetweenBullets = .3f;
    
    protected override bool ApplyToPlayer(Player thePickerUpper) {
        if (thePickerUpper.bulletSpawner.timeBetweenBullets <= timeBetweenBullets) { return false; }
        
        thePickerUpper.bulletSpawner.timeBetweenBullets = 0.3f;
        return true;
    }
}
