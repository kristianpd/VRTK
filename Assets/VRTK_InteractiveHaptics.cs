// Interactive Haptics|Interactions|30101
namespace VRTK
{
    using System;
    using UnityEngine;
    using UnityEngine.Events;
    
    /// <summary>
    /// The Interactive Haptics script is attached on the same GameObject as an Interactable Object script and provides customizable haptic feedback curves for more detailed interactions.
    /// </summary>
    [AddComponentMenu("VRTK/Scripts/Interactions/VRTK_InteractiveHaptics")]
    public class VRTK_InteractiveHaptics : MonoBehaviour
    {
        /// <summary>
        /// The types of interaction that can be performed with Interactive Haptics.
        /// </summary>
        public enum InteractionType
        {
            WhileTouching,
            WhileGrabbing,
            WhileUsing
        }

        [Header("Haptic Response")]

        [Tooltip("The type of interaction on which to apply this curve. If multiple interactions are needed, add multiple InteractiveHaptics components.")]
        public InteractionType interactionType;

        [Tooltip("Denotes the curve in which the normalized input will be evaluated. The output of the curve is the strength of the haptic feedback from 0 to 1.")]
        public AnimationCurve strengthCurve;

        [Tooltip("Denotes the curve in which the normalized input will be evaluated. The output of the curve is the interval between fastestPulseInterval and slowestPulseInterval represented by the curve.")]
        public AnimationCurve pulseIntervalCurve;
        
        [Tooltip("The fastest possible pulse interval. Maps to the upper boundary (1) of the pulse interval curve. This value should be the lower of the two as a lower number represents less time and more frequent intervals.")]
        public float fastestPulseInterval;

        [Tooltip("The slowest possible pulse interval. Maps to the lower boundary (0) of the pulse interval curve. This value should be the higher of the two as a higher number represents more time and less frequent intervals.")]
        public float slowestPulseInterval;

        [Tooltip("The minimum change in the normalized value provided by the input class to effect haptic feedback. Since it's nearly impossible to hold an object completely still, this threshold helps prevent minor variations in the normalized value from triggering a pulse.")]
        public float sensitivityThreshold = 0.01f;

        [Header("Custom Settings")]

        [Tooltip("The Interactable Object to initiate the haptics from. If this is left blank, then the Interactable Object will need to be on the current or a parent GameObject.")]
        public VRTK_InteractableObject objectToAffect;

        [Tooltip("The Interactive Haptics Input object to get the input value from. If this is left blank, then the interactable object will need to be on the current GameObject.")]
        public VRTK_InteractiveHapticsInput interactiveHapticsInput;
        
        private float lastPulseTime;

        private float lastNormalizedValue;
        
        private VRTK_ControllerReference controller;

        protected virtual void OnEnable()
        {
            objectToAffect = (objectToAffect != null ? objectToAffect : GetComponentInParent<VRTK_InteractableObject>());

            if (objectToAffect != null)
            {
                VRTK_InteractableObject.InteractionType start, stop;

                if (interactionType == InteractionType.WhileTouching)
                {
                    stop = VRTK_InteractableObject.InteractionType.Untouch;
                    start = VRTK_InteractableObject.InteractionType.Touch;
                }
                else if (interactionType == InteractionType.WhileGrabbing)
                {
                    stop = VRTK_InteractableObject.InteractionType.Ungrab;
                    start = VRTK_InteractableObject.InteractionType.Grab;
                }
                else
                {
                    stop = VRTK_InteractableObject.InteractionType.Unuse;
                    start = VRTK_InteractableObject.InteractionType.Use;
                }

                objectToAffect.SubscribeToInteractionEvent(stop, StopHaptics);
                objectToAffect.SubscribeToInteractionEvent(start, StartHaptics);
            }
            else
            {
                VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_FROM_GAMEOBJECT, "VRTK_InteractiveHaptics", "VRTK_InteractableObject", "the same or parent"));
            }

            interactiveHapticsInput = (interactiveHapticsInput != null ? interactiveHapticsInput : GetComponent<VRTK_InteractiveHapticsInput>());

            if(interactiveHapticsInput != null)
            {
                interactiveHapticsInput.InputProvided += Interact;  
            }
            else
            {
                VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_FROM_GAMEOBJECT, "VRTK_InteractiveHaptics", "VRTK_InteractiveHapticsInput", "the same"));
            }
        }

        protected virtual void OnDisable()
        {
            if (objectToAffect != null)
            {
                VRTK_InteractableObject.InteractionType start, stop;

                if (interactionType == InteractionType.WhileTouching)
                {
                    stop = VRTK_InteractableObject.InteractionType.Untouch;
                    start = VRTK_InteractableObject.InteractionType.Touch;
                }
                else if (interactionType == InteractionType.WhileGrabbing)
                {
                    stop = VRTK_InteractableObject.InteractionType.Ungrab;
                    start = VRTK_InteractableObject.InteractionType.Grab;
                }
                else
                {
                    stop = VRTK_InteractableObject.InteractionType.Unuse;
                    start = VRTK_InteractableObject.InteractionType.Use;
                }

                objectToAffect.UnsubscribeFromInteractionEvent(stop, StopHaptics);
                objectToAffect.UnsubscribeFromInteractionEvent(start, StartHaptics);
            }

            if(interactiveHapticsInput != null)
            {
                interactiveHapticsInput.InputProvided -= Interact;
            }
        }

        protected virtual void StopHaptics(object sender, InteractableObjectEventArgs e)
        {
            controller = null;
        }

        protected virtual void StartHaptics(object sender, InteractableObjectEventArgs e)
        {
            controller = VRTK_ControllerReference.GetControllerReference(e.interactingObject);
        }
        
        /// <summary>
        /// The Pulse method will trigger a haptic pulse at the appropriate interval and strength as specified by the interval and strength curves at the normalizedValue position.
        /// </summary>
        /// <param name="normalizedValue">The position along the interval and strength that correspond to the appropriate haptic response.</param>
        public void Interact(float normalizedValue)
        {
            if(controller == null)
            {
                return;
            }

            float normalizedValueDelta = Mathf.Abs(lastNormalizedValue - normalizedValue);

            if(normalizedValueDelta >= sensitivityThreshold)
            {
                lastNormalizedValue = normalizedValue;

                float pulseStrength = strengthCurve.Evaluate(normalizedValue);
                
                float pulseInterval = slowestPulseInterval + ((fastestPulseInterval - slowestPulseInterval) * pulseIntervalCurve.Evaluate(normalizedValue));

                if(Time.time - lastPulseTime >= pulseInterval)
                {
                    VRTK_ControllerHaptics.TriggerHapticPulse(controller, pulseStrength);
                    lastPulseTime = Time.time;
                }
            }
        }
    }
}