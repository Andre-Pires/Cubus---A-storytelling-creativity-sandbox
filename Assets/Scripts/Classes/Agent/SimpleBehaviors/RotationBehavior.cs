﻿using Assets.Scripts.Classes.Helpers;
using UnityEngine;

namespace Assets.Scripts.Classes.Agent.SimpleBehaviors
{
    public class RotationBehavior : Behavior
    {
        public Configuration.Transitions RotateTransition;
        public float Orientation;
        public float FinalOrientation;
        public float RotationAmount;
        public Configuration.RotationDirection RotationDirection;


        public RotationBehavior(float multiplier, bool behaviorDriveActive = true) : base(multiplier, behaviorDriveActive)
        {
            BehaviorType = Configuration.Behaviors.Rotate;
        }

        //this function randomizes the Behavior
        public override void PrepareBehavior(Body body, int repetitions, float duration)
        {
            var transitionsCount = Configuration.Instance.AvailableTransitions.Count;

            //rotation Behavior
            Configuration.Transitions rotTransition =
                Configuration.Instance.AvailableTransitions[Random.Range(0, transitionsCount)];

            RotationDirection = Configuration.RotationDirection.Random;
            RotationAmount = Random.Range(-540.0f, 540.0f);
            float randomDirection = Random.Range(0, 100) > 50 ? 1 : -1;
            switch (RotationDirection)
            {
                case Configuration.RotationDirection.Left:
                    RotationAmount *= -1.0f;
                    break;
                case Configuration.RotationDirection.Right:
                    RotationAmount *= 1.0f;
                    break;
                case Configuration.RotationDirection.Alternating:
                    RotationAmount *= randomDirection;
                    break;
                case Configuration.RotationDirection.Random:
                    RotationAmount *= randomDirection;
                    break;
            }

            Orientation = body.CurrentRotation;
            RotateTransition = rotTransition;
            FinalOrientation = Orientation + RotationAmount;

            if (rotTransition == Configuration.Transitions.Instant)
            {
                BehaviorDuration = 0.0f;
            }
            else
            {
                BehaviorDuration = duration;
            }

            
            MaxBehaviorRepetitions = repetitions;
            CurrentBehaviorRepetition = 1;
            AnimationIntervalTime = BehaviorDuration / MaxBehaviorRepetitions;
        }

        //this function allows to customize the Behavior in the mind
        public void PrepareBehavior(Body body, float rotationAmount, Configuration.RotationDirection direction, Configuration.Transitions rotationTransition, int repetitions, float duration)
        {
            
            RotationAmount = rotationAmount;
            RotationDirection = direction;
            float randomDirection = Random.Range(0, 100) > 50 ? 1 : -1;
            switch (RotationDirection)
            {
                case Configuration.RotationDirection.Left:
                    RotationAmount *= -1.0f;
                    break;
                case Configuration.RotationDirection.Right:
                    RotationAmount *= 1.0f;
                    break;
                case Configuration.RotationDirection.Alternating:
                    RotationAmount *= randomDirection;
                    break;
                case Configuration.RotationDirection.Random:
                    RotationAmount *= randomDirection;
                    break;
            }

            Orientation = body.CurrentRotation;
            RotateTransition = rotationTransition;
            FinalOrientation = Orientation + RotationAmount;


            if (rotationTransition == Configuration.Transitions.Instant)
            {
                BehaviorDuration = 0.0f;
            }
            else
            {
                BehaviorDuration = duration;
            }

            MaxBehaviorRepetitions = repetitions;
            CurrentBehaviorRepetition = 1;
            AnimationIntervalTime = BehaviorDuration / MaxBehaviorRepetitions;
        }


        public override void ApplyBehavior(Body agentBody)
        {
            float rotationX = agentBody.transform.rotation.eulerAngles.x;
            float rotationZ = agentBody.transform.rotation.eulerAngles.z;

            switch (RotateTransition)
            {
                case Configuration.Transitions.Linear:
                    var lerp = (Time.time - StartTime)/AnimationIntervalTime;
                    agentBody.transform.rotation = Quaternion.Slerp(agentBody.transform.rotation,
                        Quaternion.Euler(0, FinalOrientation, 0), lerp);
                    break;
                case Configuration.Transitions.Instant:
                    agentBody.transform.eulerAngles = new Vector3(rotationX, FinalOrientation, rotationZ);
                    break;
                case Configuration.Transitions.EaseIn:
                {
                    Interpolate.Function easeFunction = Interpolate.Ease(Interpolate.EaseType.EaseInQuint);
                    float currentRotation = easeFunction(Orientation, FinalOrientation - Orientation,
                        Time.time - StartTime, AnimationIntervalTime);

                    agentBody.transform.eulerAngles = new Vector3(rotationX, currentRotation, rotationZ);
                    break;
                }
                case Configuration.Transitions.EaseOut:
                {
                    Interpolate.Function easeFunction = Interpolate.Ease(Interpolate.EaseType.EaseOutQuint);
                    float currentRotation = easeFunction(Orientation, FinalOrientation - Orientation,
                        Time.time - StartTime, AnimationIntervalTime);

                    agentBody.transform.eulerAngles = new Vector3(rotationX, currentRotation, rotationZ);
                    break;
                }
                case Configuration.Transitions.EaseInOut:
                {
                    float totalTime = AnimationIntervalTime / 2;

                    if (Time.time - StartTime <= totalTime)
                    {
                        Interpolate.Function easeFunction = Interpolate.Ease(Interpolate.EaseType.EaseInQuint);
                        float distance = FinalOrientation - Orientation;
                        float timeElapsed = Time.time - StartTime;
                        float currentRotation = easeFunction(Orientation, distance, timeElapsed, totalTime);
                        agentBody.transform.eulerAngles = new Vector3(rotationX, currentRotation, rotationZ);

                        //Debug.Log("easing in: " + easeFunction(Orientation, distance, timeElapsed, totalTime));
                    }
                    else
                    {
                        Interpolate.Function easeFunction = Interpolate.Ease(Interpolate.EaseType.EaseOutQuint);
                        float distance = -(FinalOrientation - Orientation);
                        float timeElapsed = Time.time - StartTime - totalTime;
                        float currentRotation = easeFunction(FinalOrientation, distance, timeElapsed, totalTime);
                        agentBody.transform.eulerAngles = new Vector3(rotationX, currentRotation, rotationZ);

                        //Debug.Log("easing out: " + easeFunction(FinalOrientation, distance, timeElapsed, totalTime));
                    }
                    break;
                }
            }

            if ((Time.time - StartTime) > AnimationIntervalTime)
            {
                if (CurrentBehaviorRepetition == MaxBehaviorRepetitions)
                {
                    IsOver = true;
                    FinalizeEffects(agentBody);
                    //Debug.Log("Behavior ended");
                    return;
                }

                //if rotation alternates always invert the previous orientation
                if (RotationDirection == Configuration.RotationDirection.Alternating)
                {
                    if (CurrentBehaviorRepetition + 1 == MaxBehaviorRepetitions)
                    {
                        Orientation = FinalOrientation;
                        FinalOrientation = agentBody.CurrentRotation;
                    }
                    else
                    {
                        float segmentFinalOrientation = FinalOrientation;
                        RotationAmount *= -1.0f;
                        FinalOrientation = agentBody.CurrentRotation + RotationAmount;
                        Orientation = segmentFinalOrientation;
                    }

                }
                else
                {
                    Orientation = FinalOrientation;
                    FinalOrientation = Orientation + RotationAmount;
                }

                CurrentBehaviorRepetition++;
                StartTime = Time.time;
            }

        }

        public override void FinalizeEffects(Body body)
        {
            body.CurrentRotation = FinalOrientation;
        }

    }
}