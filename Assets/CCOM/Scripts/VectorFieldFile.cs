using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace CCOM.Flow
{
    public class VectorFieldFile : FlowFile
    {
        public UnityEngine.Object _vf = null;

        BinaryReader _dataStream = null;

        protected int _sizeX, _sizeY, _sizeZ;

        private bool _vectorDataFormat = false;


        // Start is called before the first frame update
        void Start()
        {
            Load();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void Load()
        {
            if (_dataStream == null)
                LoadDataStream();
            if (!_metadataLoaded)
                LoadMetadata();
            if (!_dataLoaded)
                LoadVectorField();
        }

        public override void LoadMetadata()
        {
            if (_metadataLoaded)
            {
                Debug.Log("Metadata already loaded...");
                return;
            }

            if (_dataStream == null)
            {
                _dataStream = LoadDataStream();
                if (_dataStream == null)
                {
                    Debug.Log("Could not load VectorField file data stream...");
                    return;
                }
            }

            char[] fourCC = _dataStream.ReadChars(4);

            Debug.Log(fourCC[0].ToString() + fourCC[1].ToString() + fourCC[2].ToString() + fourCC[3].ToString());

            if (fourCC[3] == 'V')            
                _vectorDataFormat = true;
            
            Debug.Log("Vector Field File format type is " + (_vectorDataFormat ? "VECTOR" : "FLOAT"));

            _sizeX = _dataStream.ReadUInt16();
            _sizeY = _dataStream.ReadUInt16();
            _sizeZ = _dataStream.ReadUInt16();

            _xMin = _yMin = _zMin = 0;

            _xMax = _sizeX - 1;
            _yMax = _sizeY - 1;
            _zMax = _sizeZ - 1;
            
            _xRange = _xMax - _xMin + 1;
            _yRange = _yMax - _yMin + 1;
            _zRange = _zMax - _zMin + 1;
            
            _largestDim = Mathf.Max(_xRange, _yRange, _zRange);

            _points = new Vector3[_sizeX * _sizeY * _sizeZ];
            _velocities = new float[_sizeX * _sizeY * _sizeZ];

            _metadataLoaded = true;
        }

        void LoadVectorField()
        {

            if (!_metadataLoaded)
                LoadMetadata();

            if (_dataLoaded)
                return;

            _minVel = float.MaxValue;
            _maxVel = float.MinValue;

            for (int x = 0; x < _sizeX; x++)
            {
                for (int y = 0; y < _sizeY; y++)
                {
                    for (int z = 0; z < _sizeZ; z++)
                    {
                        int ind = GetIndex(x, y, z);

                        //Debug.Log(ind);

                        if (_vectorDataFormat)
                        {
                            float u = _dataStream.ReadSingle();
                            float v = _dataStream.ReadSingle();
                            float w = _dataStream.ReadSingle();

                            _points[ind] = new Vector3(u, v, w);

                            //Debug.Log($"[{x}][{y}][{z}] = {_points[ind]}");

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
                        }
                    }//end for z
                }//end for y
            }//end for x
            print(_minVel + " - " + _maxVel);
            print("Loaded: " + _points.Length + " points");
            _dataLoaded = true;
        }

        public override Vector3 Sample(Vector3 position)
        {
            float xmap = ((position.x - _xMin) / _xRange) * (float)(_sizeX - 1);
            float ymap = ((position.y - _yMin) / _yRange) * (float)(_sizeY - 1);
            float zmap = ((position.z - _zMin) / _zRange) * (float)(_sizeZ - 1);

            int x0, y0, z0, x1, y1, z1;
            float x, y, z;
            x = y = z = 0f;

            if (xmap <= 0f)
            {
                xmap = 0f;
                x0 = x1 = 0;
            }
            else if (xmap >= (float)(_sizeX - 1))
            {
                xmap = (float)(_sizeX - 1);
                x0 = x1 = _sizeX - 1;
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
            else if (ymap >= (float)(_sizeY - 1))
            {
                ymap = (float)(_sizeY - 1);
                y0 = y1 = _sizeY - 1;
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
            else if (zmap >= (float)(_sizeZ - 1))
            {
                zmap = (float)(_sizeZ - 1);
                z0 = z1 = _sizeZ - 1;
            }
            else
            {
                z0 = Mathf.FloorToInt(zmap);
                z1 = Mathf.CeilToInt(zmap);
                z = zmap - Mathf.Floor(zmap);
            }

            Vector3 Vxyz;

            Vxyz = _points[GetIndex(x0, y0, z0)] * (1f - x) * (1f - y) * (1f - z) +
                   _points[GetIndex(x1, y0, z0)] * x * (1f - y) * (1f - z) +
                   _points[GetIndex(x0, y1, z0)] * (1f - x) * y * (1f - z) +
                   _points[GetIndex(x0, y0, z1)] * (1f - x) * (1f - y) * z +
                   _points[GetIndex(x1, y0, z1)] * x * (1f - y) * z +
                   _points[GetIndex(x0, y1, z1)] * (1f - x) * y * z +
                   _points[GetIndex(x1, y1, z0)] * x * y * (1f - z) +
                   _points[GetIndex(x1, y1, z1)] * x * y * z;

            return Vxyz;
        }

        BinaryReader LoadDataStream()
        {
            if (!_vf)
                return null;
            TextAsset asset = Resources.Load(_vf.name) as TextAsset;
            Stream s = new MemoryStream(asset.bytes);
            return new BinaryReader(s);
        }

        int GetIndex(int x, int y, int z)
        {
            return x * _sizeY * _sizeZ + y * _sizeY + z;
        }
    }
}