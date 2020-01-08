using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Unity.Map;
using Mapbox.Utils;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace MapboxTest
{
    public class LocationServiceTest : MonoBehaviour
    {
        [SerializeField] private AbstractMap _map;
        [SerializeField] private Transform _player;
        [SerializeField] private Text _errorText;

        void Start()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation)) {
                Permission.RequestUserPermission(Permission.FineLocation);
            }
#endif
        }

        void Update()
        {
            if (Input.location.isEnabledByUser)
            {
                switch (Input.location.status)
                {
                    case LocationServiceStatus.Stopped:
                        Input.location.Start();
                        break;

                    case LocationServiceStatus.Running:
                        var lastData = Input.location.lastData;
                        var geo = new Vector2d(lastData.latitude, lastData.longitude);
                        var pos = _map.GeoToWorldPosition(geo, false);
                        pos.y = 0;
                        _player.position = pos;
                        _map.UpdateMap();
                        break;
                }
            }

            _errorText.gameObject.SetActive(!Input.location.isEnabledByUser);
        }
    }
}
