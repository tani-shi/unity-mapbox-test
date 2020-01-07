using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;

namespace MapboxTest.AbstractMapTest.ExtentOptionsTest
{
    public class AbstractMapExtentCameraBoundsTest : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private AbstractMap _map;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private int _visibleBuffer;
        [SerializeField] private int _disposeBuffer;

        void Start()
        {
            var options = new CameraBoundsTileProviderOptions();
            options.camera = _mainCamera;
            options.visibleBuffer = _visibleBuffer;
            options.disposeBuffer = _disposeBuffer;

            _map.SetExtent(MapExtentType.CameraBounds);
            _map.SetExtentOptions(options);
        }

        void Update()
        {
            var pos = _mainCamera.transform.position;
            pos.x += _moveSpeed * Time.deltaTime;
            _mainCamera.transform.position = pos;
        }
    }
}
