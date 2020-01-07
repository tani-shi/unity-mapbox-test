using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

namespace MapboxTest
{
    public class LocationServiceTest : MonoBehaviour
    {
        [SerializeField] private AbstractMap _map;
        [SerializeField] private Transform _player;

        void Start()
        {
            if (Input.location.isEnabledByUser)
            {
                Input.location.Start();
            }
            else
            {
                Debug.LogError("Failed to start getting location infos, change your settings to enable location services.");
            }
        }

        void Update()
        {
            if (Input.location.status == LocationServiceStatus.Running)
            {
                var lastData = Input.location.lastData;
                var geo = new Vector2d(lastData.latitude, lastData.longitude);
                _player.position = _map.GeoToWorldPosition(geo, false);
            }
        }
    }
}
