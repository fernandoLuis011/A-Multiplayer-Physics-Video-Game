using UnityEngine;

public class Character : MonoBehaviour
{
    private int weight;
    private const int BASE_WEIGHT = 10;
    private Transform throwingHand;
    private Transform wieldingHand;

    private bool throwingHandCheck;
    private Movement movement; // might not need
    //private Pickable p; // might not need

    // Start is called before the first frame update
    void Start()
    {
        weight = BASE_WEIGHT;
        //If character is girl, ThrowablePlacementG.
        throwingHand = GameObject.Find("ThrowablePlacementB").GetComponent<Transform>();
        //if character is boy, ThrowablePlacementB.
        //throwingHand = GameObject.Find("ThrowablePlacementG").GetComponent<Transform>();
        movement = gameObject.AddComponent<Movement>();
        //p = gameObject.AddComponent<Pickable>();
        throwingHandCheck = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!throwingHandCheck)
        {
            //p.Interact();
            weight += 0; // add the weight of the item being picked up.       
        }
    }

    public int GetWeight(){
        return weight;
    }

    public void SetWeight(int weight){
        this.weight = weight;
    }
}