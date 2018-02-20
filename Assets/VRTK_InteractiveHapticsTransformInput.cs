// Interactive Haptics Input|Interactions|30103
namespace VRTK
{
    using UnityEngine;

    /// <summary>
    /// The Interactive Haptics script is attached on the same GameObject as an Interactable Object script and provides customizable haptic feedback curves for more detailed interactions.
    /// </summary>
    public class VRTK_InteractiveHapticsTransformInput : VRTK_InteractiveHapticsInput
    {
        /// <summary>
        /// The types of change on the transform that can be observed.
        /// </summary>
        public enum TransformType
        {
            Position,
            Rotation,
            Scale
        }

        /// <summary>
        /// The dimensions of the vector that can be observed.
        /// </summary>
        public enum Dimension
        {
            X,
            Y,
            Z
        }

        /// <summary>
        /// The coordinate types that can be observed. 
        /// </summary>
        public enum Coordinates
        {
            Local,
            World
        }

        /// <summary>
        /// Specifies the type of transform change that will fire input events.
        /// </summary>
        public TransformType transformType;

        /// <summary>
        /// The dimension of the observed vector (Position, Rotation, or Scale) that will be watched for changes.
        /// </summary>
        public Dimension dimension;

        /// <summary>
        /// The coordinates of the vector to consider.
        /// </summary>
        public Coordinates coordinates;

        /// <summary>
        /// The start boundary of the dimension being watched.
        /// </summary>
        public float startValue;

        /// <summary>
        /// The end boundary of the the dimension being watched.
        /// </summary>
        public float endValue;

        private Vector3 lastVector;
        
        protected virtual void FixedUpdate()
        {
            Vector3 currentVector = GetTransformVector();
            if (currentVector != lastVector)
            {
                lastVector = currentVector;                
                OnInputProvided(GetNormalizedTargetDimension(currentVector));
            }
        }        

        protected virtual float GetNormalizedTargetDimension(Vector3 vector)
        {
            float min = (startValue > endValue) ? endValue : startValue;
            float max = (startValue > endValue) ? startValue : endValue;
            return Mathf.Abs((max - GetTargetDimensionValue(vector)) / (max - min));
        }

        private Vector3 GetTransformVector()
        {
            Vector3 vector;

            if (transformType == TransformType.Position)
            {
                vector = (coordinates == Coordinates.Local) ? transform.localPosition : transform.position;
            }
            else if (transformType == TransformType.Rotation)
            {
                vector = (coordinates == Coordinates.Local) ? transform.localEulerAngles : transform.eulerAngles;
            }
            else
            {
                vector = (coordinates == Coordinates.Local) ? transform.localScale : transform.lossyScale;
            }

            return vector;
        }

        private float GetTargetDimensionValue(Vector3 vector)
        {
            float value;

            if(dimension == Dimension.X)
            {
                value = vector.x;
            }
            else if (dimension == Dimension.Y)
            {
                value = vector.y;
            }
            else
            {
                value = vector.z;
            }

            return value;
        }
    }
}