using UnityEngine;
using System.Collections;


namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarUserControl : MonoBehaviour
    {
        private CarController _carController;
        private CarInputs _carInputs;

        private void Awake()
        {
            _carController = GetComponent<CarController>();
            _carInputs = new CarInputs(_carController);
        }

        private void FixedUpdate()
        {
            _carInputs.UpdateValues();

            _carController.Move(
                _carInputs.SteeringAngle,
                _carInputs.Throttle,
                _carInputs.Brake,
                0f);
        }
    }
}
