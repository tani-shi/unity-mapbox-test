using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Map;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Map.TileProviders;

namespace MapboxTest.AbstractMapTest.ExtentOptionsTest
{
    public class AbstractMapExtentCustomTest : AbstractTileProvider
    {
        [SerializeField] private AbstractMap _mapObject;
        [SerializeField] private float _extentInterval;
        [SerializeField] private int _maxVisibleBuffer;

        private bool _initialized;

        void Start()
        {
            _mapObject.TileProvider = this;
        }

        public override void OnInitialized()
        {
            _currentExtent.activeTiles = new HashSet<UnwrappedTileId>();
            _initialized = true;
        }

        public override void UpdateTileExtent()
        {
            if (!_initialized)
            {
                return;
            }

            _currentExtent.activeTiles.Clear();
            var centerTile = TileCover.CoordinateToTileId(_map.CenterLatitudeLongitude, _map.AbsoluteZoom);
            _currentExtent.activeTiles.Add(new UnwrappedTileId(_map.AbsoluteZoom, centerTile.X, centerTile.Y));

            var west = Mathf.Clamp(Mathf.FloorToInt((Time.time + (_extentInterval * 3)) / (_extentInterval * 4)), 0, _maxVisibleBuffer);
            var north = Mathf.Clamp(Mathf.FloorToInt((Time.time + (_extentInterval * 2)) / (_extentInterval * 4)), 0, _maxVisibleBuffer);
            var east = Mathf.Clamp(Mathf.FloorToInt((Time.time + _extentInterval) / (_extentInterval * 4)), 0, _maxVisibleBuffer);
            var south = Mathf.Clamp(Mathf.FloorToInt((Time.time) / (_extentInterval * 4)), 0, _maxVisibleBuffer);

            for (int x = (centerTile.X - west); x <= (centerTile.X + east); x++)
            {
                for (int y = (centerTile.Y - north); y <= (centerTile.Y + south); y++)
                {
                    _currentExtent.activeTiles.Add(new UnwrappedTileId(_map.AbsoluteZoom, x, y));
                }
            }

            OnExtentChanged();
        }

        public override void UpdateTileProvider()
        {
            UpdateTileExtent();
        }

        public override bool Cleanup(UnwrappedTileId tile)
        {
            return (!_currentExtent.activeTiles.Contains(tile));
        }
    }
}
