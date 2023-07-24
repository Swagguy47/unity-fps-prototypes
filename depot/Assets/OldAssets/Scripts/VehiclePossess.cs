
//using UnityEditor;
using UnityEngine;


public class VehiclePossess : MonoBehaviour
{
    public int VehicleType; //0-ground, 1-aircraft, 2-mech
    [HideInInspector] public VehicleBrain Vehicle;
    [HideInInspector] public AircraftBrain Aircraft;
    [HideInInspector] public CharacterBrain Mech;
}

//Custom Inspector
//[CustomEditor(typeof(VehiclePossess))]
public class PossessEditor : MonoBehaviour//: Editor
{
    /*public override void OnInspectorGUI()
    {
        //Initialize stuff
        base.DrawDefaultInspector();

        VehiclePossess Possessor = (VehiclePossess) target;


        //Dropdown vehicle type menu
        //string[] options = new string[]
        //{
        //        "Ground Vehicle", "Aircraft", "Mech",
        //};
        //Possessor.VehicleType = EditorGUILayout.Popup("Vehicle Type:", Possessor.VehicleType, options);

        //The different input fields
        if (Possessor.VehicleType == 0) //ground vehicle
        {
            Possessor.Vehicle = EditorGUILayout.ObjectField("VehicleBrain", Possessor.Vehicle, typeof(VehicleBrain), true) as VehicleBrain;
        }
        else if (Possessor.VehicleType == 1) //aircraft
        {
            Possessor.Aircraft = EditorGUILayout.ObjectField("AircraftBrain", Possessor.Aircraft, typeof(AircraftBrain), true) as AircraftBrain;
        }
        else //mech
        {
            Possessor.Mech = EditorGUILayout.ObjectField("CharacterBrain", Possessor.Mech, typeof(CharacterBrain), true) as CharacterBrain;
        }
    }*/
}