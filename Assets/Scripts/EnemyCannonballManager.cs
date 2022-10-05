using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCannonballManager : MonoBehaviour
{
    // Holds list of cannonballs and keeps track of the current index
    // Essentially a global pool of cannonballs that enemies can use
    public List<GameObject> cannonballs;
    public int cannonballIndex;
}
