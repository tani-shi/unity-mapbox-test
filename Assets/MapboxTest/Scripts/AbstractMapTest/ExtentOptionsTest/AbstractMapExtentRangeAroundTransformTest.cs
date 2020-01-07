using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

namespace MapboxTest.AbstractMapTest.ExtentOptionsTest
{
    public class AbstractMapExtentRangeAroundTransformTest : MonoBehaviour
    {
        [SerializeField] private AbstractMap _map;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private Transform _target;
        [SerializeField] private int _visibleBuffer;
        [SerializeField] private int _disposeBuffer;

        void Start()
        {
            var options = new RangeAroundTransformTileProviderOptions();
            options.targetTransform = _target;
            options.visibleBuffer = _visibleBuffer;
            options.disposeBuffer = _disposeBuffer;

            _map.SetExtent(MapExtentType.RangeAroundTransform);
            _map.SetExtentOptions(options);
        }

        void Update()
        {
            var position = _target.position;
            position.x += _moveSpeed * Time.deltaTime;
            _target.position = position;
        }
    }
}
