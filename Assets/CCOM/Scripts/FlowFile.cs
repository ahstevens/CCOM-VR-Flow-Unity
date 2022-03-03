/*MIT License

Copyright(c) 2018 Vili Volčini / viliwonka

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CCOM.Flow {    
        abstract public class FlowFile : MonoBehaviour{

        protected Vector3[] _points;
        protected float[] _velocities;

        protected float _xMin, _xMax, _xRange;
        protected float _yMin, _yMax, _yRange;
        protected float _zMin, _zMax, _zRange;
        protected float _largestDim;

        public float _verticalExaggeration = 1f;

        protected float _minVel = 0f;
        protected float _maxVel = 0f;

        protected bool _metadataLoaded = false;
        protected bool _dataLoaded = false;

        void Start() {
           
        }

        void Update()
        {
            // update flowgrid with vertical exaggeration
            if (gameObject.transform.localScale.z != _zRange * _verticalExaggeration)
            {
                RealignVerticalExaggeration();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(this.transform.position, Vector3.one);
        }

        public virtual void Load()
        {

        }

        public abstract void LoadMetadata();


        public void RealignVerticalExaggeration()
        {
            float zScale = _zRange * _verticalExaggeration;

            gameObject.transform.localScale = new Vector3(
                gameObject.transform.localScale.x,
                gameObject.transform.localScale.y,
                zScale);

            Vector3 newPos = GetMidpoint();
            newPos.z *= _verticalExaggeration;
            gameObject.transform.localPosition = newPos;

            Vector3 alignmentPos = GetMidpoint();
            alignmentPos.x *= -gameObject.transform.parent.localScale.x;
            alignmentPos.y *= -gameObject.transform.parent.localScale.y;
            alignmentPos.z *= -gameObject.transform.parent.localScale.z * _verticalExaggeration;

            gameObject.transform.parent.parent.localPosition = alignmentPos;

            //HairySlice[] slices = gameObject.transform.parent.GetComponentsInChildren<HairySlice>();
            //
            //float adjustAmount = zScale / gameObject.transform.localScale.z;
            //if (slices.Length > 0)
            //{
            //    foreach (HairySlice slice in slices)
            //    {
            //        Vector3 pos = slice.transform.localPosition;
            //        pos.z *= adjustAmount;
            //        slice.transform.localPosition = pos;
            //        print("Slice Adjusted");
            //    }
            //}         
        }

        public virtual Vector3 Sample(Vector3 pos)
        {
            return Vector3.zero;
        }

        public virtual bool HasChanged()
        {
            return false;
        }

        public bool IsLoaded()
        {
            return _dataLoaded;
        }

        public virtual bool InBounds(Vector3 pos)
        {
            if (!_metadataLoaded)
                return false;

            return pos.x > _xMin && pos.x < _xMax && pos.y > _yMin && pos.y < _yMax && pos.z > _zMin && pos.z < _zMax;
        }

        public Vector3 GetMaxBound()
        {
            return new Vector3(_xMax, _yMax, _zMax);
        }

        public Vector3 GetMinBound()
        {
            return new Vector3(_xMin, _yMin, _zMin);
        }

        public Vector3 GetRange()
        {
            return new Vector3(_xRange, _yRange, _zRange);
        }

        public float GetMaxDim()
        {
            return _largestDim;
        }

        public float GetMinVelocity()
        {
            return _minVel;
        }

        public float GetMaxVelocity()
        {
            return _maxVel;
        }

        public float GetVelocityRange()
        {
            return _maxVel - _minVel;
        }

        public Vector3 GetMidpoint()
        {
            return new Vector3(
                    _xMin + _xRange * 0.5f,
                    _yMin + _yRange * 0.5f,
                    _zMin + _zRange * 0.5f);
        }
    }
}