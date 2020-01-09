using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Unity;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using Mapbox.Directions;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace MapboxTest
{
    public class DirectionsTest : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private AbstractMap _map;
        [SerializeField] private Transform _startPos;
        [SerializeField] private Transform _endPos;
        [SerializeField] private GameObject _directions;
        [SerializeField] private GameObject _destPoint;
        [SerializeField] private Image _loadingPanel;
        [SerializeField] private Transform _player;
        [SerializeField] private Text _remainingDistanceText;
        [SerializeField] private float _intervalTimeToUpdate;
        [SerializeField] private float _maxZoom;
        [SerializeField] private float _minZoom;
        [SerializeField] private float _wheelSensitivity;
        [SerializeField] private float _pitchSensitivity;
        [SerializeField] private float _mouseSlideSensitivity;
        [SerializeField] private float _touchSlideSensitivity;
        [SerializeField] private float _doubleTouchThresholdTime;
        [SerializeField] private float _clickThresholdTime;
        [SerializeField] private float _distanceToStartDirection;

        const string kMouseWheelAxisName = "Mouse ScrollWheel";
        const string kDirectionsName = "direction waypoint  entity";

        private LocationInfo _lastData;
        private float _lastTouchDistance = -1;
        private Vector3 _lastMousePos;
        private float _lastTouchTime;

        void Start()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation)) {
                Permission.RequestUserPermission(Permission.FineLocation);
            }
#endif

            StartCoroutine(UpdateLocation());
        }

        void Update()
        {
            if (_loadingPanel.gameObject.activeSelf)
            {
                if (GameObject.Find(kDirectionsName) != null)
                {
                    _loadingPanel.gameObject.SetActive(false);
                }
                return;
            }

#if UNITY_EDITOR
            var axis = Input.GetAxis(kMouseWheelAxisName);
            if (axis != 0f)
            {
                OnZoom(axis * _wheelSensitivity);
            }

            if (Input.GetMouseButton(0))
            {
                if (_lastMousePos != Vector3.zero && _lastMousePos != Input.mousePosition)
                {
                    OnSlide(new Vector2(Input.mousePosition.x - _lastMousePos.x, Input.mousePosition.y - _lastMousePos.y) * _mouseSlideSensitivity);
                }
                _lastMousePos = Input.mousePosition;
            }
            else
            {
                _lastMousePos = Vector2.zero;
            }

            if (Input.GetMouseButtonDown(0))
            {
                _lastTouchTime = Time.time;
            }
            else if (Input.GetMouseButtonUp(0) && Time.time - _lastTouchTime < _clickThresholdTime)
            {
                OnDoubleTouch(Input.mousePosition);
            }
#else
            if (Input.touchCount == 2)
            {
                var distance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                if (_lastTouchDistance >= 0)
                {
                    if (_lastTouchDistance != distance)
                    {
                        OnZoom((distance - _lastTouchDistance) * _pitchSensitivity);
                    }
                }
                _lastTouchDistance = distance;
            }
            else
            {
                _lastTouchDistance = -1;

                if (Input.touchCount == 1)
                {
                    if (Input.GetTouch(0).deltaPosition.sqrMagnitude > 0f)
                    {
                        OnSlide(Input.GetTouch(0).deltaPosition * _touchSlideSensitivity);
                    }
                    if (Input.GetTouch(0).phase == TouchPhase.Ended)
                    {
                        if (Time.time - _lastTouchTime < _doubleTouchThresholdTime)
                        {
                            OnDoubleTouch(Input.GetTouch(0).position);
                        }
                        _lastTouchTime = Time.time;
                    }
                }
            }
#endif
        }

        IEnumerator UpdateLocation()
        {
            while (Input.location.isEnabledByUser)
            {
                switch (Input.location.status)
                {
                    case LocationServiceStatus.Stopped:
                        Input.location.Start();
                        break;

                    case LocationServiceStatus.Running:
                        if (Input.location.lastData.latitude != _lastData.latitude ||
                            Input.location.lastData.longitude != _lastData.longitude)
                        {
                            _lastData = Input.location.lastData;
                            OnChangedLocation(Input.location.lastData);
                        }
                        break;
                }

                yield return new WaitForSeconds(_intervalTimeToUpdate);
            }
        }

        void OnChangedLocation(LocationInfo location)
        {
            var geo = new Vector2d(location.latitude, location.longitude);
            var pos = _map.GeoToWorldPosition(geo);
            _player.position = pos;

            if (_startPos.position == _endPos.position)
            {
                _startPos.position = pos;
                _endPos.position = pos;
                transform.position = pos;
                _map.UpdateMap();
            }
            else
            {
                _startPos.position = pos;
                QueryDirections(pos, _endPos.position);
            }
        }

        void OnSlide(Vector2 delta)
        {
            transform.position += new Vector3(-delta.x, 0, -delta.y);
        }

        void OnZoom(float sub)
        {
            var pos = _mainCamera.transform.localPosition;
            pos.z = Mathf.Clamp(pos.z + sub, _minZoom, _maxZoom);
            _mainCamera.transform.localPosition = pos;
        }

        void OnDoubleTouch(Vector2 pos)
        {
            RaycastHit hit;
            if (!Physics.Raycast(_mainCamera.ScreenPointToRay(pos), out hit))
            {
                return;
            }

#if UNITY_EDITOR
            var startPos = _map.GeoToWorldPosition(_map.CenterLatitudeLongitude);
#else
            var startPos = _map.GeoToWorldPosition(new Vector2d(_lastData.latitude, _lastData.longitude));
#endif
            var endPos = new Vector3(hit.point.x, 0, hit.point.z);

            _startPos.position = startPos;
            _endPos.position = endPos;

            _destPoint.SetActive(true);

            var color = _loadingPanel.color;
            color.a = 0.5f;
            _loadingPanel.color = color;
            _loadingPanel.gameObject.SetActive(true);

            var direction = GameObject.Find(kDirectionsName);
            if (direction != null)
            {
                Destroy(direction);
            }

            QueryDirections(startPos, endPos);
        }

        void QueryDirections(Vector3 startPos, Vector3 endPos)
        {
            var wp = new Vector2d[]
            {
                startPos.GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale),
                endPos.GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale),
            };
            var directionResource = new DirectionResource(wp, RoutingProfile.Walking);
            directionResource.Steps = true;
            MapboxAccess.Instance.Directions.Query(directionResource, HandleDirectionsResponse);
        }

        void HandleDirectionsResponse(DirectionsResponse response)
        {
            var distance = response.Routes.Sum(o => o.Distance);
            _remainingDistanceText.text = string.Format("Distance: {0}m", distance);
        }
    }
}
