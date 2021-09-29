using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace CCOM.Flow
{
    public class FlowGrid : FlowFile
    {
        public UnityEngine.Object _flowfile = null;
        public bool _usesZInsteadOfDepth = false;

        BinaryReader _dataStream = null;

        bool[] _water; // does the cell contain water?
        public float[] _depthLevels;
        public float[] _timeSteps;

        protected float _tMin, _tMax, _tRange;
        protected int _xCells, _yCells, _zCells, _nTimesteps;

        public float _time = 0f;
        float _lastTime = 0f;
        public bool _play = false;
        public float _replayFactor = 1f;
        bool _timeChanged = false;

        // Start is called before the first frame update
        void Start()
        {
            Load();
        }

        // Update is called once per frame
        void Update()
        {            
            _time = Mathf.Clamp(_time, _tMin, _tMax);

            if (_nTimesteps > 1)
            {
                if (_lastTime != _time)
                {
                    _lastTime = _time;
                    _timeChanged = true;
                }
                else if (_timeChanged)
                    _timeChanged = false;

                if (_play)
                {
                    _time = (_time + Time.deltaTime * _replayFactor) % _tMax;

                    if (_time < _tMin)
                        _time += _tMax;

                }

                _time = Mathf.Clamp(_time, _tMin, _tMax);
            }
        }

        public override void Load()
        {
            if (_dataStream == null)
                LoadDataStream();
            if (!_metadataLoaded)
                LoadMetadata();
            if (!_dataLoaded)
                LoadFlowGrid();
        }

        public override void LoadMetadata()
        {
            if (_metadataLoaded)
                return;

            if (_dataStream == null)
            {
                _dataStream = LoadDataStream();
                if (_dataStream == null)
                    return;
            }

            _xMin = _dataStream.ReadSingle();
            _xMax = _dataStream.ReadSingle();
            _xRange = _xMax - _xMin;
            _xCells = _dataStream.ReadInt32();

            _yMin = _dataStream.ReadSingle();
            _yMax = _dataStream.ReadSingle();
            _yRange = _yMax - _yMin;
            _yCells = _dataStream.ReadInt32();

            if (_usesZInsteadOfDepth)
            {
                _zMin = _dataStream.ReadSingle();
                _zMax = _dataStream.ReadSingle();
            }
            else
            {
                _zMin = -1f;
                _zMax = 0f;
            }
            _zCells = _dataStream.ReadInt32();

            _nTimesteps = _dataStream.ReadInt32();

            _points = new Vector3[_xCells * _yCells * _zCells * _nTimesteps];
            _velocities = new float[_xCells * _yCells * _zCells * _nTimesteps];
            _water = new bool[_xCells * _yCells * _zCells * _nTimesteps];
            _depthLevels = new float[_zCells];
            _timeSteps = new float[_nTimesteps];

            for (int i = 0; i < _zCells; i++)
            {
                _depthLevels[i] = _dataStream.ReadSingle();
            }

            _zMin = _depthLevels[0];
            _zMax = _depthLevels[_depthLevels.Length - 1];
            _zRange = _zMax - _zMin;
            _largestDim = Mathf.Max(_xRange, _yRange, _zRange);

            for (int i = 0; i < _nTimesteps; i++)
            {
                _timeSteps[i] = _dataStream.ReadSingle();
            }

            _tMin = _timeSteps[0];
            _tMax = _timeSteps[_nTimesteps - 1];
            _tRange = _tMax - _tMin;

            _metadataLoaded = true;
        }

        void LoadFlowGrid()
        {

            if (!_metadataLoaded)
                LoadMetadata();

            if (_dataLoaded)
                return;

            _minVel = float.MaxValue;
            _maxVel = float.MinValue;

            for (int x = 0; x < _xCells; x++)
            {
                for (int y = 0; y < _yCells; y++)
                {
                    for (int z = 0; z < _zCells; z++)
                    {
                        for (int t = 0; t < _nTimesteps; ++t)
                        {
                            int ind = GetIndex(x, y, z, t);
                            _water[ind] = _dataStream.ReadInt32() == 1;
                            float u = _dataStream.ReadSingle();
                            float v = _dataStream.ReadSingle();
                            float w = 0f;

                            if (_usesZInsteadOfDepth)
                                w = _dataStream.ReadSingle();

                            _points[ind] = new Vector3(u, v, w);

                            //if (z == _zCells / 2)
                            //    print("Read Point " + ind + ": " + _points[ind]);
                            _velocities[ind] = (float)(Mathf.Sqrt(
                                _points[ind].x * _points[ind].x +
                                _points[ind].y * _points[ind].y +
                                _points[ind].z * _points[ind].z));

                            if (_velocities[ind] < _minVel)
                                _minVel = _velocities[ind];

                            if (_velocities[ind] > _maxVel)
                                _maxVel = _velocities[ind];
                        }//end for t
                    }//end for z
                }//end for y
            }//end for x
            print(_minVel + " - " + _maxVel);
            print("Loaded: " + _points.Length + " points");
            _dataLoaded = true;
        }

        public override Vector3 Sample(Vector3 position)
        {
            float xmap = ((position.x - _xMin) / _xRange) * (float)(_xCells - 1);
            float ymap = ((position.y - _yMin) / _yRange) * (float)(_yCells - 1);
            float zmap = ((position.z - _zMin) / _zRange) * (float)(_zCells - 1);
            float tmap = ((_time - _tMin) / _tRange) * (float)(_nTimesteps - 1);

            int x0, y0, z0, x1, y1, z1;
            float x, y, z;
            x = y = z = 0f;

            int timestepLow = Mathf.FloorToInt(tmap);
            int timestepHigh = Mathf.CeilToInt(tmap);
            float timeDiff = tmap - Mathf.Floor(tmap);

            if (xmap <= 0f)
            {
                xmap = 0f;
                x0 = x1 = 0;
            }
            else if (xmap >= (float)(_xCells - 1))
            {
                xmap = (float)(_xCells - 1);
                x0 = x1 = _xCells - 1;
            }
            else
            {
                x0 = Mathf.FloorToInt(xmap);
                x1 = Mathf.CeilToInt(xmap);
                x = xmap - Mathf.Floor(xmap);
            }

            if (ymap <= 0f)
            {
                ymap = 0f;
                y0 = y1 = 0;
            }
            else if (ymap >= (float)(_yCells - 1))
            {
                ymap = (float)(_yCells - 1);
                y0 = y1 = _yCells - 1;
            }
            else
            {
                y0 = Mathf.FloorToInt(ymap);
                y1 = Mathf.CeilToInt(ymap);
                y = ymap - Mathf.Floor(ymap);
            }

            if (zmap <= 0f)
            {
                zmap = 0f;
                z0 = z1 = 0;
            }
            else if (zmap >= (float)(_zCells - 1))
            {
                zmap = (float)(_zCells - 1);
                z0 = z1 = _zCells - 1;
            }
            else
            {
                z0 = Mathf.FloorToInt(zmap);
                z1 = Mathf.CeilToInt(zmap);
                z = zmap - Mathf.Floor(zmap);
            }

            Vector3 Vxyz;

            Vxyz = _points[GetIndex(x0, y0, z0, (int)timestepLow)] * (1f - x) * (1f - y) * (1f - z) +
                   _points[GetIndex(x1, y0, z0, (int)timestepLow)] * x * (1f - y) * (1f - z) +
                   _points[GetIndex(x0, y1, z0, (int)timestepLow)] * (1f - x) * y * (1f - z) +
                   _points[GetIndex(x0, y0, z1, (int)timestepLow)] * (1f - x) * (1f - y) * z +
                   _points[GetIndex(x1, y0, z1, (int)timestepLow)] * x * (1f - y) * z +
                   _points[GetIndex(x0, y1, z1, (int)timestepLow)] * (1f - x) * y * z +
                   _points[GetIndex(x1, y1, z0, (int)timestepLow)] * x * y * (1f - z) +
                   _points[GetIndex(x1, y1, z1, (int)timestepLow)] * x * y * z;

            if (timestepLow != timestepHigh)
            {
                Vector3 Vxyz_after = _points[GetIndex(x0, y0, z0, (int)timestepHigh)] * (1f - x) * (1f - y) * (1f - z) +
                   _points[GetIndex(x1, y0, z0, (int)timestepHigh)] * x * (1f - y) * (1f - z) +
                   _points[GetIndex(x0, y1, z0, (int)timestepHigh)] * (1f - x) * y * (1f - z) +
                   _points[GetIndex(x0, y0, z1, (int)timestepHigh)] * (1f - x) * (1f - y) * z +
                   _points[GetIndex(x1, y0, z1, (int)timestepHigh)] * x * (1f - y) * z +
                   _points[GetIndex(x0, y1, z1, (int)timestepHigh)] * (1f - x) * y * z +
                   _points[GetIndex(x1, y1, z0, (int)timestepHigh)] * x * y * (1f - z) +
                   _points[GetIndex(x1, y1, z1, (int)timestepHigh)] * x * y * z;

                Vxyz = Vxyz * (1f - timeDiff) + Vxyz_after * timeDiff;
            }

            return Vxyz;
        }

        public override bool HasChanged()
        {
            return IsPlaying() || TimeChanged();
        }

        BinaryReader LoadDataStream()
        {
            if (!_flowfile)
                return null;
            TextAsset asset = Resources.Load(_flowfile.name) as TextAsset;
            Stream s = new MemoryStream(asset.bytes);
            return new BinaryReader(s);
        }

        public int GetTimestepCount()
        {
            return _nTimesteps;
        }

        public bool IsPlaying()
        {
            return _nTimesteps > 0 && _play;
        }

        public bool TimeChanged()
        {
            return _timeChanged;
        }

        public bool IsWater(int x, int y, int z, int timeStep)
        {
            return _water[GetIndex(x, y, z, timeStep)];
        }

        int GetIndex(int x, int y, int z, int t = 0)
        {
            return ((t * _xCells + x) * _yCells + y) * _zCells + z;
        }
    }
}