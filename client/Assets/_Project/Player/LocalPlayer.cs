using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using SpacetimeDB.Types;
using SpacetimeDB;

public class LocalPlayer : MonoBehaviour
{
    [SerializeField] private GameObject cameraRig;

    public static LocalPlayer instance;
    public string Username { set { nameElement.text = value; } }

    public TMP_Text nameElement;

    private Vector2 movementVec;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        cameraRig.SetActive(true);
        PlayerMovementController.Local = GetComponent<PlayerMovementController>();
        PlayerAnimator.Local = GetComponentInChildren<PlayerAnimator>(true);        
    }


    private float? lastUpdateTime;
    private void FixedUpdate()
    {
      float? deltaTime = Time.time - lastUpdateTime;
      bool hasUpdatedRecently = deltaTime.HasValue && deltaTime.Value < 1.0f;
      bool isConnected = SpacetimeDBClient.instance.IsConnected();

      if (hasUpdatedRecently || !isConnected)
      {
          return;
      }

      lastUpdateTime = Time.time;
      var p = PlayerMovementController.Local.GetModelTransform().position;

      Reducer.UpdatePlayerPosition(new StdbVector3
          {
            X = p.x,
            Y = p.y,
            Z = p.z,
          },
          PlayerMovementController.Local.GetModelTransform().rotation.eulerAngles.y, 
          PlayerMovementController.Local.IsMoving());
    }

    public void SetMove(Vector3 vec) => movementVec = vec;
}
