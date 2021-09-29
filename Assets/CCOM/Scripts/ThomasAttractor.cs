using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CCOM.Flow
{
    public class ThomasAttractor : FlowFile
    {
        public float b = 0.32899f;

        float _maxVelSq = 0f;
        float _minVelSq = float.MaxValue;

        // Start is called before the first frame update
        void Start()
        {
            Load();
        }

        public override void LoadMetadata()
        {
            _xMin = -4f;
            _xMax = 4f;
            _xRange = _xMax - _xMin;

            _yMin = -4f;
            _yMax = 4f;
            _yRange = _yMax - _yMin;

            _zMin = -4f;
            _zMax = 4f;
            _zRange = _zMax - _zMin;

            _largestDim = Mathf.Max(_xRange, _yRange, _zRange);

            _metadataLoaded = true;
        }

        public override void Load()
        {
            LoadMetadata();
            _dataLoaded = true;
        }

        public override Vector3 Sample(Vector3 pos)
        {
            Vector3 uvw = Thomas(pos);

            CheckVelocity(uvw);

            return uvw;
        }

        Vector3 Thomas(Vector3 p)
        {
            return new Vector3(
                Mathf.Sin(p.y) - b * p.x,
                Mathf.Sin(p.z) - b * p.y,
                Mathf.Sin(p.x) - b * p.z
            );
        }

        void CheckVelocity(Vector3 uvw)
        {
            float magSq = uvw.sqrMagnitude;

            if (magSq < _minVelSq)
            {
                _minVelSq = magSq;
                _minVel = Mathf.Sqrt(_minVelSq);
            }

            if (magSq > _maxVelSq)
            {
                _maxVelSq = magSq;
                _maxVel = Mathf.Sqrt(_maxVelSq);
            }
        }
    }
}