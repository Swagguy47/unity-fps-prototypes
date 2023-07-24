using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechRepossessor : MonoBehaviour
{
    private CharacterBrain Driver;
    [SerializeField] private Interactable Interactor;
    [SerializeField] private VehicleBrain MechSeat;
    [SerializeField] private CharacterBrain Mech;
    [SerializeField] private CameraTrack CamTrack;

    private void Start()
    {
        Mech.CameraTrackOverride = CamTrack;
    }

    public void EnterMech()
    {
        if (Driver == null && !Interactor.Interactor.IgnoreVehicles)
        {
            Driver = Interactor.Interactor;
            MechSeat.VehicleEnter();
            Driver.Unposess();
            //Mech.Possess();
            PlayerCallback.PlayerBrain.ForceRepossess(Mech);
        }
    }

    public void ExitMech()
    {
        if (Driver != null)
        {
            Mech.Unposess();
            Interactor.Interactor = Driver; //set vehicle exitTrig variable to just be interactor :P
            MechSeat.VehicleExit();
            //Driver.Possess();
            PlayerCallback.PlayerBrain.ForceRepossess(Driver);
            Driver = null;
        }
    }
}
