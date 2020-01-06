using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;

namespace MapboxTest.AbstractMapTest {
    public class AbstractMapZoomTest : MonoBehaviour
    {
        [SerializeField] private AbstractMap _map;
        [SerializeField] private float _zoomTime;
        [SerializeField] private float _zoomStart;
        [SerializeField] private float _zoomEnd;

        void Update()
        {
            var zoom = Mathf.Lerp(_zoomStart, _zoomEnd, (Time.time % _zoomTime) / _zoomTime);
            Debug.Log("Zoom:" + zoom);
            _map.SetZoom(zoom);

            // Must be invoked, if changed MapOptions.
            _map.UpdateMap();
        }
    }

}