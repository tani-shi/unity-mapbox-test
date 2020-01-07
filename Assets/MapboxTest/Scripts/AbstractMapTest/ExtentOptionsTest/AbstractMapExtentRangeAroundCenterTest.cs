using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

namespace MapboxTest.AbstractMapTest.ExtentOptionsTest
{
    public class AbstractMapExtentRangeAroundCenterTest : MonoBehaviour
    {
        [SerializeField] private AbstractMap _map;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private int _west;
        [SerializeField] private int _north;
        [SerializeField] private int _east;
        [SerializeField] private int _south;

        void Start()
        {
            var options = new RangeTileProviderOptions();
            options.west = _west;
            options.north = _north;
            options.east = _east;
            options.south = _south;

            _map.SetExtent(MapExtentType.RangeAroundCenter);
            _map.SetExtentOptions(options);
        }

        void Update()
        {
            var latlong = _map.CenterLatitudeLongitude;
            latlong.y += _moveSpeed * Time.deltaTime;
            if (latlong.y > Constants.LongitudeMax) {
                latlong.y = latlong.y - (Constants.LongitudeMax * 2);
            } else if (latlong.y < -Constants.LongitudeMax) {
                latlong.y = latlong.y + (Constants.LongitudeMax * 2);
            }
            _map.SetCenterLatitudeLongitude(latlong);
            _map.UpdateMap();
        }
    }
}
