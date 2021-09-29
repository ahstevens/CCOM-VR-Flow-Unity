using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CCOM.Flow
{
    public class LorenzAttractor : FlowFile
    {
        public float r = 28f;
        public float σ = 10f;
        public float β = 8f / 3f;

        float _maxVelSq = 0f;
        float _minVelSq = float.MaxValue;

        // Start is called before the first frame update
        void Start()
        {
            Load();
        }

        public override void LoadMetadata()
        {
            _xMin = -100f;
            _xMax = 100f;
            _xRange = _xMax - _xMin;

            _yMin = -100f;
            _yMax = 100f;
            _yRange = _yMax - _yMin;

            _zMin = -100f;
            _zMax = 100f;
            _zRange = _zMax - _zMin;

            _largestDim = 200f;

            _metadataLoaded = true;
        }

        public override void Load()
        {
            LoadMetadata();
            _dataLoaded = true;
        }

        public override Vector3 Sample(Vector3 pos)
        {
            Vector3 uvw = LorenzStep(pos) - pos;

            CheckVelocity(uvw);

            return uvw;
        }

        Vector3 LorenzStep(Vector3 p)
        {
            return new Vector3(
                σ * (p.y - p.x),
                p.x * (r - p.z) - p.y,
                p.x * p.y - β * p.z
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

        public override bool InBounds(Vector3 pos)
        {
            return true;
        }
    }
}